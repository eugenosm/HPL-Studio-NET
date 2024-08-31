using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HPLStudio
{

    internal class HpmStruct
    {

        public static Regex GetSetRe = new Regex(
            @"(?<copy>(?<regd>@?\w+)\{_get_set\((?<fieldd>\w+)\)\}\s*=\s*(?<regs>@?\w+)(\{_get_set\((?<fields>\w+)\)\}))|(?<get>(?<dest>@?\w+)(?<dest_idx>\[\w+\])?\s*=\s*(?<reg>@?\w+)(\{_get_set\((?<field>\w+)\)\}))|(?<set>(?<reg>@?\w+)\{_get_set\((?<field>\w+)\)\}\s*=\s*(?<val>@?\w+)(?<val_idx>\[\w+\])?)"
            , RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex StructRe = 
            new Regex(@"#struct\s*(?<structDef>.*?)(?<comment>\s*;[\w\t ~!@#$%^&*()\-+=`""""\\/|№?\{\[\]\}':;<>,.]*)?\n(?<structBody>.*?)#ends"
                , RegexOptions.Singleline | RegexOptions.Compiled);


        private static readonly Regex FieldRe =
            new Regex(
                @"\s*(?<field>\w+(\s*=\s*\d+)?)\s*(?<comment>;.*)?"
                , RegexOptions.Singleline | RegexOptions.Compiled);

        private static string GetSetEvaluator(Match x)
        {
            if (x.Groups["copy"].Success)
            {
                /*
                    get: R0=RegS, R0=&0x1F000, R0=>>0xC                   
                    set: RegD=&0xFFFFFFFFFFFE0FFF, R0=R0, R0=<<0xC, R0=&0x1F000, RegD=|R0                                    
                 */

                var getterName = $"_get_{x.Groups["fields"].Value}";
                var getter = Macro.GlobalStorage[getterName];
                var get = getter.Apply($"{getterName}(R0)");
                var setterName = $"_set_{x.Groups["fieldd"].Value}";
                var setter = Macro.GlobalStorage[setterName];
                var set = setter.Apply($"{setterName}(R0)").Replace("R0=R0, ", "");
                return $"{get}, {set}";

            }


            else if (x.Groups["get"].Success)
            {
                var getterName = $"_get_{x.Groups["field"].Value}";
                var getter = Macro.GlobalStorage[getterName];
                var arg = x.Groups["dest"].Value;
                return getter.Apply($"{getterName}({arg})");
            }

            else if (x.Groups["set"].Success)
            {
                var setterName = $"_set_{x.Groups["field"].Value}";
                var setter = Macro.GlobalStorage[setterName];
                var arg = x.Groups["val"].Value;
                //var cmt = $"; {x.Groups[8].Value} = {arg}\n";
                // R1=R0,R8=&0xFFFFFFFFFFFFF01F, R0=R1, R0=<<0x5, R0=&0xFE0, R8=|R0
                return arg == "R0"  
                    ? setter.Apply($"R1=R0, {setterName}(R1), R0=R1").Replace("R0=R1, ", "")
                    : setter.Apply($"{setterName}({arg})");
            }
            return x.Value;
        }

        public static string ApplyGetterSetter(string source)
        {
            return GetSetRe.Replace(source, GetSetEvaluator);
        }

        /// <summary>
        /// Обрабатывает все определения структур в исходном файле, формируя по ним список переменных (полей) и
        /// макросов (геттеров/сеттеров). Добавляет макросы и переменные в хранилища.
        /// Возвращает исходную строку с закомментированными определениями структур.
        /// Отдельно возвращает строку с закомментированными сформированными макросами и дефайнами.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="vars"></param>
        /// <param name="macros"></param>
        /// <returns></returns>
        public static (ErrorRec, string, string) ProcessStructDefs(string source, KeyValList.KeyValList vars = null,
            Dictionary<string, Macro> macros = null)
        {
            macros ??= Macro.GlobalStorage;
            vars ??= Preprocessor.Variables;
            var error = new ErrorRec();

            var structGetSetMacroDefs = new List<string>();
            structGetSetMacroDefs.Clear();
            StringBuilder macroCommentedBuilder = new ();
            string macroCommented;


            var dest = StructRe.Replace(source, x =>
            {
                if (error.Code != ErrorRec.ErrCodes.EcOk) return x.Value;
                var (line, col) = Preprocessor.FindLineNoInBlob(source, x.Index);

                var parsedLine = x.Groups["structDef"].Value.Split('=');
                if (parsedLine.Length < 2)
                {
                    error = new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, line, "");
                    return x.Value;
                }
                var identifierHead = parsedLine[0].Trim();
                var bitPos = 0;
                var valueHeader = parsedLine[1].Trim();
                var isArray = valueHeader[0] == '@';

                /*
                 * setter => _set_Struct_Field(R0) => Struct.Field = R0;
                 * getter => _get_Struct_field() => R0=(Struct.Field)
                 *
                 */
                if (Preprocessor.CheckIdentifierIsFree(ref vars, identifierHead))
                {
                    if (!Preprocessor.CheckPrevDefined(ref vars, identifierHead))
                    {
                        var hdrLen = x.Groups["structBody"].Index - x.Index;
                        var structBody = x.Groups["structBody"].Value.Split(
                            Preprocessor.CrLfSeparators, StringSplitOptions.RemoveEmptyEntries );

                        foreach (var fieldDefStr in structBody)
                        {
                            line++;
                            if (fieldDefStr.Trim().StartsWith(";")) // no field def, just a comment, skip it
                            {
                                continue;
                            }

                            var fieldDefMatch = FieldRe.Match(fieldDefStr);
                            if (!fieldDefMatch.Success) // incorrect field definition
                            {
                                error = new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, line, "");
                                return x.Value;
                            }

//                            result.Add($"; {fieldDefStr}");

                            var fieldDef = fieldDefMatch.Groups["field"].Value;
                            var parsedField = fieldDef.Split('=');

                            string identifier;
                            string value;

                            var fieldSize = (parsedField.Length > 1)
                                ? int.TryParse(parsedField[1].Trim(), out var fs) ? fs : -1
                                : 1;

                            if (isArray || fieldSize != 1)
                            {
                                var aStructFieldName = parsedField[0].Trim();
                                identifier = identifierHead + "." + aStructFieldName;
                                if (fieldSize is < 1 or > 32)
                                {
                                    error = new ErrorRec(ErrorRec.ErrCodes.EcErrorInParameters, line, "");
                                    return x.Value;
                                }
                                var field = (isArray)
                                    ? new ArrStructField(valueHeader, bitPos, fieldSize)
                                    : new StructField(valueHeader, bitPos, fieldSize);

                                AddFieldMacro($"_set_{identifierHead}_{aStructFieldName}(arg)", field, field.Setter);
                                AddFieldMacro($"_get_{identifierHead}_{aStructFieldName}(dst)", field, field.Getter);
                                value = $"{{_get_set({identifierHead}_{aStructFieldName})}}";
                                Preprocessor.PushDef(ref vars, value, value);
                                bitPos += fieldSize;
                            }
                            else
                            {
                                identifier = identifierHead + "." + fieldDef.Trim();
                                value = $"[{bitPos}]";
                                bitPos++;
                            }

                            if (Preprocessor.CheckIdentifierIsFree(ref vars, identifier))
                            {

                                Preprocessor.PushDef(ref vars, identifier, valueHeader + value);
                                //result.Add("; " + fieldDefStr);
                            }
                            else
                            {
                                error = new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, line, "")
                                {
                                    Info = identifier
                                };
                                return x.Value;
                            }

                            if (!isArray)
                            {
                                for (var fieldBit = 0; fieldBit < fieldSize; fieldBit++)
                                {
                                    var regBit = $"[{bitPos - fieldSize + fieldBit}]";
                                    var ident = identifier + $"[{fieldBit}]";
                                    if (Preprocessor.CheckIdentifierIsFree(ref vars, ident))
                                    {
                                        Preprocessor.PushDef(ref vars, ident, valueHeader + regBit);
                                    }

                                }
                            }
                        }

                        if (Preprocessor.CheckIdentifierIsFree(ref vars, identifierHead))
                        {
                            Preprocessor.PushDef(ref vars, identifierHead, valueHeader);
                        }
                        else
                        {
                            (line, col) = Preprocessor.FindLineNoInBlob(source, x.Index);
                            error = new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, line, "")
                            {
                                Info = identifierHead
                            };
                            return x.Value;
                            
                        }

                        //result.Add(";" + line);
                        //result.Add("");

                        (error, macroCommented) = Macro.ProcessMacroDefs(string.Join("\n", structGetSetMacroDefs));
                        macroCommentedBuilder.Append("\n");
                        macroCommentedBuilder.Append(macroCommented);
                        structGetSetMacroDefs.Clear();
                        // result.AddRange(macroCommented.Split('\n'));
                        // return string.Join("\n", result);

                        return Preprocessor.CommentAllLines(x.Value);
                    }
                }
                error = new ErrorRec(ErrorRec.ErrCodes.EcErrorSubDefinePreviouslyDefined, line, "");
                return x.Value;
            });
            return (error, dest, macroCommentedBuilder.ToString());

            void AddFieldMacro(string macroName, StructField field, string func)
            {
                structGetSetMacroDefs.Add($"#macro {macroName}");
                structGetSetMacroDefs.Add(func);
                structGetSetMacroDefs.Add("#endm");
                structGetSetMacroDefs.Add(";---------------------------");
                structGetSetMacroDefs.Add("");
            }
        }

    }

    internal class StructField
    {
        // Temporary Programmator Registers
        internal static string Tr0 = "R0";
        internal static string Tr1 = "R1";
        public string Getter => getter;
        public string Setter => setter;

        protected string identifier;
        protected int offset;
        protected ulong mask;

        protected string getter;
        protected string setter;

        public StructField()
        {

        }

        public StructField(string identifier, int offs, int size)
        {
            this.identifier = identifier;
            offset = offs % 64;
            mask = ((ulong)(1 << size) - 1) << offs;
            getter = _generateGetStructField();
            setter = _generateSetStructField();
        }

        private string _generateGetStructField()
        {
            return offset > 0
                ? $"dst={identifier}, dst=&0x{mask:X}, dst=>>0x{offset:X}"
                : $"dst={identifier}, dst=&0x{mask:X}";

        }

        private string _generateSetStructField()
        {
            return offset > 0
                ? $"{identifier}=&0x{(~mask):X}, R0=arg, R0=<<0x{offset:X}, R0=&0x{mask:X}, {identifier}=|R0"
                : $"{identifier}=&0x{(~mask):X}, R0=arg, R0=&0x{mask:X}, {identifier}=|R0";
        }


    }

    internal class ArrStructField : StructField
    {

        private readonly int _idx;
        private readonly int _shift;
        private readonly int _byteCnt;

        public ArrStructField(string identifier, int offs, int size)
        {
            base.identifier = identifier;
            _shift = offs % 8;
            _idx = offs / 8;
            mask = ((1ul << size) - 1) << _shift;
            _byteCnt = _getByteCount(size, _shift);
            getter = _generateGetArrStructField();
            setter = _generateSetArrStructField();
        }

        private string _generateGetArrStructField1Byte()
            => (_shift == 0) ? ((byte)mask == 255)
                    ? $" dst={identifier}[{_idx}]"
                    : $" dst={identifier}[{_idx}], dst=&0x{(byte)mask:X}"
                : $" dst={identifier}[{_idx}], dst=&0x{(byte)mask:X}, dst=>>{_shift}";

        private string _generateGetArrStructField()
        {
            var r = _generateGetArrStructField1Byte();
            for (var i = 1; i < _byteCnt; i++) // MAX: 4 bytes + 2 cuts
            {
                mask >>= 8;
                if ((mask & 0xFFFFFFFF) == 0) break;
                if ((byte)mask == 255)
                    r += $", {Tr1}={identifier}[{_idx + i}], {Tr1}=<<{i * 8 - _shift}, dst=|{Tr1}";
                else
                    r += $", {Tr1}={identifier}[{_idx + i}], {Tr1}=&0x{((byte)mask):X}, dst=<<{i * 8 - _shift}, dst=|{Tr1}";
            }

            return r;
        }

        private string _generateSetArrStructField()
        {
            var nMask = ~(mask);
            var r = (_shift == 0)
                ? ((byte)mask == 255)
                    ? new StringBuilder($"{identifier}[{_idx}]=arg")
                    : new StringBuilder($"{Tr0}=arg, {Tr1}={identifier}[{_idx}], {Tr1}=&0x{(byte)nMask:X}, {Tr1}=|{Tr0}, {identifier}[{_idx}]={Tr1}")
                : new StringBuilder($"{Tr0}=arg, {Tr1}={identifier}[{_idx}], {Tr1}=&0x{(byte)nMask:X}, {Tr0}=<<{_shift}, {Tr1}=|{Tr0}, {identifier}[{_idx}]={Tr1}");
            for (var i = 1; i < _byteCnt; i++)
            {
                nMask >>= 8;
                r.Append((byte)nMask == 0xFF
                    ? $", {Tr0}=>>8, {identifier}[{_idx + i}]={Tr0}"
                    : $", {Tr0}=>>8, {Tr1}={identifier}[{_idx+i}], {Tr1}=&0x{(byte)nMask:X}, {Tr1}=|{Tr0}, {identifier}[{_idx+i}]={Tr1}");
            }
            return r.ToString();
        }

        protected static int _getByteCount(int size, int shift)
        {
            return size switch
            {
                1 => 1,
                < 9 => (size + shift) > 8 ? 2 : 1,
                9 => 2,
                < 17 => (size + shift) > 16 ? 3 : 2,
                17 => 3,
                < 25 => (size + shift) > 24 ? 4 : 3,
                25 => 4,
                < 33 => (size + shift) > 32 ? 5 : 4,
                _ => 0
            };

        }


    }

}