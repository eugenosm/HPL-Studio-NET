using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace HPLStudio
{


    public class ErrorRec
    {
        public enum ErrCodes
        {
            EcOk = 0, EcErrorInDefineExpression, EcErrorInIncludeExpressionOrFileNotFound,
            EcErrorIncorrectDirective, EcErrorIdentifierAlreadyDefined, EcErrorInInitSectionExpression,
            EcErrorSubDefinePreviouslyDefined, EcErrorInParameters, EcErrorInSectionName
        };
        public ErrCodes Code { get; set; }
        public int LineNo { get; set; }
        public string FileName { get; set; }

        public string Info { get; set; }
        public ErrorRec()
        {
            Code = ErrCodes.EcOk;
            LineNo = 0;
            FileName = "";
            Info = null;
        }
        public ErrorRec(ErrCodes code, int line, string file)
        {
            Code = code;
            LineNo = line;
            FileName = file;
            Info = null;
        }
        private static string ErrorTextByCode(ErrCodes code)
        {
            return code switch
            {
                ErrCodes.EcOk => "OK",
                ErrCodes.EcErrorInDefineExpression => "Error In Define Expression",
                ErrCodes.EcErrorInIncludeExpressionOrFileNotFound =>
                    "Error In Include Expression Or File Not Found",
                ErrCodes.EcErrorIncorrectDirective => "Error Incorrect Directive",
                ErrCodes.EcErrorIdentifierAlreadyDefined => "Error Identifier Already Defined",
                ErrCodes.EcErrorInInitSectionExpression => "Error in Init Section Expression",
                ErrCodes.EcErrorSubDefinePreviouslyDefined => "Error: Subdefine Previously Defined",
                ErrCodes.EcErrorInParameters => "Error Parameters Out Of Range or of Incorrect Type",
                ErrCodes.EcErrorInSectionName => "Incorrect Section Name.",
                _ => "Unknown Error"
            };
        }

        public override string ToString()
        {
            return (Info is null)
                ? $"{ErrorTextByCode(Code)}, in file <{FileName}>, at line: {LineNo}"
                : $"{ErrorTextByCode(Code)} ({Info}), in file <{FileName}>, at line: {LineNo}";
        }
    };

    internal class ParsingInfo
    {
        public static int LineNo;
    }

    internal class Preprocessor
    {
        public static KeyValList.KeyValList Variables;

        public const string SectionRegex = "^\\[[#!~]*\x22?(\\w+)\x22?\\].*$"; // @"^\s*\[(\w+|[\#\!\~]+\w+|\x22[\w\s]+\x22|[\#\!\~]+\x22[\w\s]+\x22)\].*$";

        internal static void AddPreCompiledLines ( 
            ref List<string> funcLines, ref string[] preCompiledLines, ref List<string> dest, KeyValList.KeyValList vars = null )
        {
            vars ??= Variables;

            dest.AddRange(preCompiledLines);
            var funcs = ExtractFuncDefs(funcLines, 0,  vars);
            foreach (var f in funcs.Where(f => vars.IndexOfKey(f) < 0))
            {
                vars.Add(f, f);
            }
            /*

        foreach (var line in funcLines.Select(x => x.Trim()))
        {
            var pp = line.IndexOf(']');
            if (line.IndexOf('[') == 0 && pp  > 0)
            {
                lines = lines.Substring(1,pp-1).Trim();
                PushDef(ref vars, lines, lines);
            }
        }
            */
        }


        private static ErrorRec DoDef(
            ref List<string> source, ref List<string> dest, ref string trimLine, KeyValList.KeyValList vars = null)
        {
            vars ??= Variables;
            var parsedLine = trimLine.Substring(4).Split(new []{ '=' }, 2);
            if (parsedLine.Length < 2)
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, ParsingInfo.LineNo, "");
            }
            var identifier = parsedLine[0].Trim();
            var value = parsedLine[1].Trim();
            if (CheckIdentifierIsFree(ref vars, identifier))
            {
                PushDef(ref vars, identifier, value);
                dest.Add(";" + source[ParsingInfo.LineNo]);
            }
            else
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, ParsingInfo.LineNo, "")
                {
                    Info = identifier
                };
            }
            return new ErrorRec();
        }

        /*
        private static ErrorRec DoMacro(
            ref List<string> source, ref List<string> dest, ref string trimLine, KeyValList.KeyValList vars = null)
        {
            vars ??= Variables;
            var header = source[ParsingInfo.LineNo];
            var macro = Macro.ParseHeader(header);

            var body = new List<string>();
            dest.Add($";{header}");
            while (ParsingInfo.LineNo < source.Count && trimLine.IndexOf("#endm", StringComparison.Ordinal) != 0)
            {
                ParsingInfo.LineNo++;
                var line = source[ParsingInfo.LineNo];
                trimLine = line.Trim();
                if (ParsingInfo.LineNo > source.Count())
                {
                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, ParsingInfo.LineNo, "");
                }
                dest.Add($";{line}");
                body.Add(trimLine);
            }
            body.RemoveAt(body.Count-1);
            macro.Body = string.Join(Environment.NewLine, body);
            if (Macro.GlobalStorage.ContainsKey(macro.Name))
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, ParsingInfo.LineNo, "") {Info = macro.Name};
            }
            Macro.GlobalStorage.Add(macro.Name, macro);

            return new ErrorRec();
        }
        */

        private static int _fieldBytes(int fieldSize, int shift = 0)
            => (fieldSize + shift) / 8 + (((fieldSize + shift) % 8) == 0 ? 0 : 1);
        

        private static ErrorRec DoInclude(ref List<string> dest, ref string trimLine, KeyValList.KeyValList vars = null)
        {
            vars ??= Variables;
            var parsedLine = trimLine.Split( '=');
            var includeFileName = parsedLine[1].Trim();
            if (System.IO.File.Exists(includeFileName))
            {
                var include = System.IO.File.ReadAllLines(includeFileName).ToList();
                var result = Compile(ref include, out var dst, vars);                           
                if (result != null && result.Code != ErrorRec.ErrCodes.EcOk)
                {
                    if (string.IsNullOrEmpty(result.FileName))
                    {
                        result.FileName = includeFileName;
                    }
                    return result;
                }
                AddPreCompiledLines(ref include, ref dst, ref dest, vars);
                include.Clear();
            }
            else
            {
                return new ErrorRec(
                    ErrorRec.ErrCodes.EcErrorInIncludeExpressionOrFileNotFound, ParsingInfo.LineNo, "");
            }
            return new ErrorRec();
        }

        public static void InitPredefinedVars(ref KeyValList.KeyValList vars)
        {
            PushDef(ref vars, "$OPERATION", "RE");     // Код операции.
            PushDef(ref vars, "$OPERATION.READ", "1");  // Значения кодов операции (USER - пользовательская операция)
            PushDef(ref vars, "$OPERATION.VERIFY", "2");
            PushDef(ref vars, "$OPERATION.WRITE", "3");
            PushDef(ref vars, "$OPERATION.USER", "4");

            PushDef(ref vars, "$VALUE", "RA");          // Используется как параметр функций, и как результат комманды PRINT=A("..")
            PushDef(ref vars, "$VALUE.Ok", "1");        //    Варианты результата PRINT=A()
            PushDef(ref vars, "$VALUE.Cancel");

            PushDef(ref vars, "#type.bin", "B");        // При описании интерфейсной части, символические имена типов отображения значений регистров
            PushDef(ref vars, "#type.check_box", "C");
            PushDef(ref vars, "#type.dec", "D");
            PushDef(ref vars, "#type.hex_editor", "E");
            PushDef(ref vars, "#type.hex", "H");
            PushDef(ref vars, "#type.list", "L");
            PushDef(ref vars, "#type.string", "S");
            PushDef(ref vars, "#type.push_button", "P");
        }

        public static (bool, string) FuncDefCheck(string line, KeyValList.KeyValList vars=null)
        {
            vars ??= Variables;

            var m = Regex.Match(line, SectionRegex);
            if (m.Success)
            {
                var identifier = m.Groups[1].Value;
                if (vars.IndexOfKey(identifier) < 0)
                {
                    PushDef(ref vars, identifier, identifier);
                }
                return (true, identifier);
            }
            
            return (false, null);
        }


        private static string CleanLine(string line)
        {
            var trimLine = line.Trim();
            var commentPos = trimLine.IndexOf(';');
            if (commentPos >= 0) trimLine = trimLine.Substring(0, commentPos);
            return trimLine;
        }
        /// <summary>
        /// Обрабатывает секцию определений. 
        /// </summary>
        /// <param name="source">Исходный код</param>
        /// <param name="dest">Обработанный код</param>
        /// <param name="vars">Переменные</param>
        /// <returns>При удаче возвращает EcOk и Номер строки с первой секцией (функцией). При ошибке возвращает строку ошибки.</returns>
        private static (int, ErrorRec) ProcessDefs(ref List<string> source, List<string> dest, KeyValList.KeyValList vars=null)
        {
            vars ??= Variables;

            for (ParsingInfo.LineNo = 0; ParsingInfo.LineNo < source.Count; ParsingInfo.LineNo++)
            {
                var line = source[ParsingInfo.LineNo];
                if (Regex.Match(line, SectionRegex).Success)
                { // найдена первая секция "кода", выходим
                    return (ParsingInfo.LineNo, new ErrorRec());
                }

                var trimLine = CleanLine(line);
                if (trimLine.Length > 0 && trimLine[0] == '#')
                {
                    if (trimLine.IndexOf("#def", StringComparison.Ordinal) == 0)
                    {
                        var r = DoDef(ref source, ref dest, ref trimLine, vars);
                        if (r.Code != ErrorRec.ErrCodes.EcOk)
                        {
                            return (ParsingInfo.LineNo, r);
                        }
                    }
                    else if (trimLine.IndexOf("#struct", StringComparison.Ordinal) == 0)
                    {
                        var r = HpmStruct.DoStruct(ref source, ref dest, ref vars, ref ParsingInfo.LineNo, ref trimLine, ref line);
                        if (r.Code != ErrorRec.ErrCodes.EcOk)
                        {
                            return (ParsingInfo.LineNo, r);
                        }
                    }
                    else if (trimLine.IndexOf("#include", StringComparison.Ordinal) == 0)
                    {
                        var r = DoInclude(ref dest, ref trimLine);
                        if (r.Code != ErrorRec.ErrCodes.EcOk)
                        {
                            return (ParsingInfo.LineNo, r);
                        }

                    }
                    else
                    {
                        return (ParsingInfo.LineNo, new ErrorRec(
                            ErrorRec.ErrCodes.EcErrorIncorrectDirective, ParsingInfo.LineNo, ""));
                    }
                }
                else
                {
                    var r = ProcessSocketAndVars(ref dest, ref vars, line, trimLine, ParsingInfo.LineNo);
                    if (r.Code != ErrorRec.ErrCodes.EcOk) return (ParsingInfo.LineNo, r);
                }
            }

            return (source.Count, new ErrorRec());
        }

        private static ErrorRec ProcessSocketAndVars(ref List<string> dest, ref KeyValList.KeyValList vars, string line, string trimLine, int idx)
        {
            dest.Add(line);
            if (trimLine.IndexOf("PIN", StringComparison.Ordinal) == 0 || trimLine.IndexOf("BUS", StringComparison.Ordinal) == 0)
            {
                // PINO = CLK,12,8
                var pinEndPos =  3;
                var c = trimLine.Length > 3 ? trimLine[3] : '*';
                if( c is 'I' or 'O' or 'G')
                {
                    pinEndPos = 4;
                }

                var identifier = trimLine.Substring(pinEndPos).Trim();
                if (identifier.IndexOf('=') == 0)
                {
                    var equPos = identifier.IndexOf(',');
                    if (equPos > 1)
                    {
                        var value = identifier.Substring(1, equPos - 1).Trim();
                        PushDef(ref vars, value, value);
                        return new ErrorRec();
                    }
                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorInInitSectionExpression, idx, "");
                }
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInInitSectionExpression, idx, "");
            }

            return new ErrorRec();
        }

        public static List<string> ExtractFuncDefs(List<string> source, int codeStart, KeyValList.KeyValList vars=null, List<string> functions=null)
        {
            vars ??= Variables;
            var funcs = functions ?? new List<string>();
            for (var i = codeStart; i < source.Count; i++)
            {
                var line = source[i];
                var (ok, identifier) = FuncDefCheck(line, vars);
                if (ok)
                {
                    funcs.Add(identifier);
                }
            }
            return funcs;
        }

        

        private static string StoreComments(string source, out List<string> comments)
        {
            var commentsRe = new Regex(";.*?\n", RegexOptions.Compiled | RegexOptions.Singleline);

            var foundComments = new List<string>();

            var r = commentsRe.Replace(source, x =>
            {
                foundComments.Add(x.Value);
                return $"{{*{foundComments.Count - 1}*}}";
            });

            comments = foundComments;
            return r;
        }

        private static string RestoreComments(string source, IReadOnlyList<string> comments)
        {
            var commentsRe = new Regex(@"\{\*(\d+)\*\}", RegexOptions.Compiled | RegexOptions.Singleline);
            return commentsRe.Replace(source, x 
                    => int.TryParse(x.Groups[1].Value, out var v) ? comments[v] : x.Value );
        }


        private static List<string> StoreStrings(ref List<string> source)
        {
            var strings = new List<string>();
            for (var i = 0; i < source.Count; i++)
            {
                var line = source[i];
                foreach (Match m in Regex.Matches(line, "(\".*?\")"))
                {
                    line = line.Replace(m.Groups[1].Value, $"{{${strings.Count}$}}");
                    strings.Add(m.Groups[1].Value);
                }
                source[i] = line;
            }

            return strings;
        }


        private static void RestoreStrings(ref List<string> source, IReadOnlyList<string> strings)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var line = source[i];
                foreach (Match m in Regex.Matches(line, @"\{\$(\d+)\$\}"))
                {
                    if (int.TryParse(m.Groups[1].Value, out var value))
                    {
                        line = line.Replace($"{{${value}$}}", strings[value]);
                    }
                }
                source[i] = line;
            }
        }


        
        public static ErrorRec Compile(ref List<string> source, out string[] dest, KeyValList.KeyValList vars = null)
        {
            vars ??= Variables;            
            if (vars.Count == 0)
            {
                InitPredefinedVars(ref vars);
                Macro.GlobalStorage ??= new Dictionary<string, Macro>();
                Macro.GlobalStorage.Clear();
            }


            var savedStrings = StoreStrings(ref source);
            var (err, sourceStr) = Macro.ProcessMacroDefs(string.Join(Environment.NewLine, source), vars);
            if (err.Code != ErrorRec.ErrCodes.EcOk)
            {
                dest = source.ToArray(); 
                return err;
            }
            var destBuf = new List<string>();

            source.Clear();
            source.AddRange(sourceStr.Split(new string[] {Environment.NewLine}, StringSplitOptions.None));

            var (codeStart, r) = ProcessDefs(ref source, destBuf, vars);
            if (r.Code != ErrorRec.ErrCodes.EcOk)
            {
                dest = destBuf.ToArray();
                return r;
            }

            // список имен, в которых не будет производиться подмена макроопределений
            // Для того чтобы в случаях, как в примере ниже, не портилось имя функции
            // #def CRC=R2
            // [_CalcCRC]
            var defs = source.Take(codeStart);
            var implementation = string.Join("\n", source.Skip(codeStart));
            implementation = Macro.GlobalStorage.Aggregate(implementation, 
                (current, kv) => kv.Value.Apply(current));

            implementation = StoreComments(implementation, out var commentsStorage);

            foreach (var kv in vars.List)
            {
                if(kv.Key == kv.Value) continue;

                var re = new Regex($@"\b{Regex.Escape(kv.Key)}\b", RegexOptions.Compiled | RegexOptions.Singleline);
                implementation = re.Replace(implementation, x => kv.Value);
            }

            implementation = HpmStruct.ApplyGetterSetter(implementation);
            implementation = RestoreComments(implementation, commentsStorage);
            destBuf.AddRange(implementation.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            RestoreStrings(ref source, savedStrings);
            RestoreStrings(ref destBuf, savedStrings);

            dest = destBuf.ToArray();
            return new ErrorRec();
        }
        //-----------------------
        public static string[] keywords = {"HPL", "ADDR", "ALLPINS", "ALTNAME", "BCOLOR", "BGCOLOR", "BREAK", "BUSI", "BUSO"
                                             ,"CONST", "CDELAY", "DATA", "EXIT", "GAP", "GET", "IENABLE", "IDISABLE"
                                             , "INFO", "INFOFILE", "IMAGE", "LOOP", "MARK", "MATRIX", "OPTIONS", "PING", "PINI", "PINO"
                                             , "PIN", "PLACE", "POWER", "PRINT", "SHAPE", "SIZE", "SPACE", "TCOLOR"
                                             , "TR", "TL", "TD", "TU", "D", "C", "L", "R", "A", "S", "E", "N", "O", "P"
                                             , "REG", "RETURN", "SOCKET", "VCC", "VPP"};

        public static string[] registers = {"$SIZE", "$MAXSIZE", "$FREQ", "$CHNS", "$AREA", "$BLOCKSIZE", "$CDELAY", "$WDELAY"
                                             , "$VERIFY", "$VERSION", "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8"
                                             , "R9", "RA", "RB", "RC", "RD", "RE", "RF", "R10", "R11", "R12", "R13", "R14"
                                             , "R15", "R16", "R17", "R18", "R19", "R20", "R21", "R22", "R23", "R24", "R25"
                                             , "R26", "R27", "R28", "R29", "R30", "R31", "R1A", "R1B", "R1C", "R1D", "R1E", "R1F" };
        public static string[] addons = {    "#def","#struct","#ends","#macro","#endm"
                                            ,"$OPERATION","$OPERATION.READ","$OPERATION.VERIFY","$OPERATION.WRITE","$OPERATION.USER"
                                            ,"$VALUE","$VALUE.Ok","$VALUE.Cancel"
                                            ,"#type","#type.bin","#type.check_box","#type.dec","#type.hex_editor","#type.hex"
                                            ,"#type.list","#type.string","#type.push_button" };

        public static bool CheckIdentifierIsFree( ref KeyValList.KeyValList vars, string identifier )
        {
            var upperIdentifier = identifier.ToUpper();

            if (keywords.Equals(upperIdentifier) )
            {
                return false;
            }
            if (registers.Equals(upperIdentifier))
            {
                return false;
            }
            return vars.IndexOfKey(identifier) < 0;
        }
 
       public static bool CheckPrevDefined(ref KeyValList.KeyValList vars, string identifier)
        {
            for (var i = 0; i < vars.Count; i++ )
            {
                if (identifier.IndexOf(vars[i], StringComparison.Ordinal) >= 0)
                {
                    return true;
                }

            }
            return false;
        }
       public static bool PushDef(ref KeyValList.KeyValList vars, string identifier, string value = "0")
        {
            
            var foundKeys = new List<string>(); 
            //var foundVals = new List<string>();
            for (var i = 0; i < vars.Count; i++)
            {
                if (identifier.IndexOf(vars[i], StringComparison.Ordinal) < 0) continue;
                vars.Insert(i,identifier,value);
                return true;

            }
            vars[identifier] = value;
            return false;
        }

    }

   
}
 