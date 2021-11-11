﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HPLStudio
{

    internal class HpmStruct
    {
        public static Regex GetSetRe = new Regex(
            @"((@?\w+)(\[\w+\])?\s*=\s*@\w+(\{_get_set\((\w+)\)\}))|(@\w+(\{_get_set\((\w+)\)\})\s*=\s*(@?\w+)(\[\w+\])?)"
            , RegexOptions.Singleline | RegexOptions.Compiled);

        private static string GetSetEvaluator(Match x)
        {
            if (x.Groups[1].Success)
            {
                var getterName = $"_get_{x.Groups[5].Value}";
                var getter = Preprocessor.macros[getterName];
                var arg = x.Groups[2].Value;
                return getter.Apply($" {getterName}({arg}) ");
            }

            if (x.Groups[6].Success)
            {
                var setterName = $"_get_{x.Groups[8].Value}";
                var setter = Preprocessor.macros[setterName];
                var arg = x.Groups[9].Value;
                return arg.StartsWith("R")  // TODO: заменить на регулярку, проверяем что это регистр
                    ? setter.Apply($" {setterName}({arg}) ")
                    : $"R0={arg},{setter.Apply($" {setterName}(R0) ")}";
            }
            return x.Value;
        }

        public static string ApplyGetterSetter(string source)
        {
            return GetSetRe.Replace(source, GetSetEvaluator);
        }

        public static ErrorRec DoStruct(ref List<string> source, ref List<string> dest, ref KeyValList.KeyValList vars
            , ref int srcLine, ref string trimLine, ref string line)
        {

            var parsedLine = trimLine.Substring(7).Split('=');
            if (parsedLine.Length < 2)
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, srcLine, "");
            }

            /*
             * setter => _set_Struct_Field(R0) => Struct.Field = R0;
             * getter => _get_Struct_field() => R0=(Struct.Field)
             *
             */

            var identifierHead = parsedLine[0].Trim();
            var bitPos = 0;
            var valueHeader = parsedLine[1].Trim();
            var isArray = valueHeader[0] == '@';
            if (Preprocessor.CheckIdentifierIsFree(ref vars, identifierHead))
            {
                if (!Preprocessor.CheckPrevDefined(ref vars, identifierHead))
                {

                    srcLine++;
                    if (srcLine > source.Count)
                    {
                        return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, srcLine, "");
                    }
                    dest.Add(";" + line);
                    line = source[srcLine];
                    trimLine = line.Trim();
                    var structFuncs = new List<string>();
                    structFuncs.Clear();
                    while (trimLine.IndexOf("#ends", StringComparison.Ordinal) != 0)
                    {
                        if (trimLine.StartsWith(";"))
                        {
                            dest.Add(line);
                        }
                        else
                        {
                            parsedLine = trimLine.Split('='); // equPos = trimLine.IndexOf('=');
                            string identifier;
                            string value;
                            var fieldSize = (parsedLine.Length > 1)
                                ? int.TryParse(parsedLine[1].Trim(), out var fs) ? fs : -1
                                : 1;

                            if (isArray || fieldSize != 1)
                            {
                                var aStructFieldName = parsedLine[0].Trim();
                                identifier = identifierHead + "." + aStructFieldName;
                                if (fieldSize is < 1 or > 32)
                                {
                                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorInParameters, srcLine, "");
                                }
                                structFuncs.Add(";---------------------------");
                                /*
                                var funcName = $"[_set_{identifierHead}_{aStructFieldName}]";

                                if (funcName.Length > 22)
                                {
                                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorInSectionName, srcLine, "")
                                    {
                                        Info = $"_set_{identifierHead}_{aStructFieldName}"
                                    };
                                }
                                */
                                var macroName = $"_set_{identifierHead}_{aStructFieldName}";
                                //structFuncs.Add(funcName);
                                //structFuncs.Add(";R0 - value");//  ;$VALUE - value, or use as parameter
                                structFuncs.Add($"#macro {macroName}(arg)");

                                var field = (isArray)
                                    ? new ArrStructField(valueHeader, bitPos, fieldSize)
                                    : new StructField(valueHeader, bitPos, fieldSize);

                                structFuncs.Add(field.Setter);
                                structFuncs.Add("#endm");
                                structFuncs.Add(";---------------------------");
                                // value = $"[_get_{identifierHead}_{aStructFieldName}]";
                                // structFuncs.Add(value);
                                macroName = $"_get_{identifierHead}_{aStructFieldName}";
                                structFuncs.Add($"#macro {macroName}(dst)");
                                //structFuncs.Add(";R0 - result");//  ;$VALUE - result
                                structFuncs.Add(field.Getter);
                                structFuncs.Add("#endm");
                                structFuncs.Add(";---------------------------");

                                value = $"{{_get_set({identifierHead}_{aStructFieldName})}}";
                                Preprocessor.PushDef(ref vars, value, value);
                                bitPos += fieldSize;
                            }
                            else
                            {
                                identifier = identifierHead + "." + trimLine;
                                value = $"[{bitPos}]";
                                bitPos++;
                            }

                            if (Preprocessor.CheckIdentifierIsFree(ref vars, identifier))
                            {

                                Preprocessor.PushDef(ref vars, identifier, valueHeader + value);
                                dest.Add(";" + line);
                            }
                            else
                            {
                                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, srcLine, "")
                                {
                                    Info = identifier
                                };
                            }

                            if (!isArray)
                            {
                                for (var bc = 0; bc < fieldSize; bc++)
                                {
                                    value = $"[{bitPos - fieldSize + bc}]";
                                    var ident = identifier + $"[{bc}]";
                                    if (Preprocessor.CheckIdentifierIsFree(ref vars, ident))
                                    {
                                        Preprocessor.PushDef(ref vars, ident, valueHeader + value);
                                    }

                                }

                            }
                        }

                        srcLine++;
                        line = source[srcLine];
                        trimLine = line.Trim();
                        if (srcLine > source.Count)
                        {
                            return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, srcLine, "");
                        }
                    }
                    Preprocessor.PushDef(ref vars, identifierHead, valueHeader);
                    dest.Add(";" + line);
                    var result = Preprocessor.Compile(ref structFuncs, out var strFuncAdd, ref vars);
                    if (result != null && result.Code != ErrorRec.ErrCodes.EcOk)
                    {
                        return result;
                    }
                    Preprocessor.AddPreCompiledLines(ref structFuncs, ref strFuncAdd, ref dest, ref vars);
                    structFuncs.Clear();
                }
                else
                {
                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorSubDefinePreviouslyDefined, srcLine, "");
                }
            }
            else
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, srcLine, "")
                {
                    Info = identifierHead
                };
                
            }
            return new ErrorRec();
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
            offset = offs % 8;
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
                    ? $"{Tr0}={identifier}[{_idx}]"
                    : $"{Tr0}={identifier}[{_idx}], {Tr0}=&0x{(byte)mask:X}"
                : $"{Tr0}={identifier}[{_idx}], {Tr0}=&0x{(byte)mask:X}, {Tr0}=>>{_shift}";

        private string _generateGetArrStructField()
        {
            var r = _generateGetArrStructField1Byte();
            for (var i = 1; i < _byteCnt; i++) // MAX: 4 bytes + 2 cuts
            {
                mask >>= 8;
                if ((mask & 0xFFFFFFFF) == 0) break;
                if ((byte)mask == 255)
                    r += $", {Tr1}={identifier}[{_idx + i}], {Tr1}=<<{i * 8 - _shift}, {Tr0}=|{Tr1}";
                else
                    r += $", {Tr1}={identifier}[{_idx + i}], {Tr1}=&0x{((byte)mask):X}, {Tr1}=<<{i * 8 - _shift}, {Tr0}=|{Tr1}";
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