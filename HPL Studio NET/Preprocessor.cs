using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text.RegularExpressions;


namespace HPLStudio
{
    internal class Preprocessor
    {
        public const string SectionRegex = "^\\[[#!~]*\x22?(\\w+)\x22?\\].*$"; // @"^\s*\[(\w+|[\#\!\~]+\w+|\x22[\w\s]+\x22|[\#\!\~]+\x22[\w\s]+\x22)\].*$";
        public class ErrorRec
        {
            public enum ErrCodes{ EcOk = 0, EcErrorInDefineExpression, EcErrorInIncludeExpressionOrFileNotFound,
                EcErrorIncorrectDirective, EcErrorIdentifierAlreadyDefined, EcErrorInInitSectionExpression,
                EcErrorSubDefinePreviouslyDefined, EcErrorInParameters
            };
            public ErrCodes errorCode;
            public int errorLine;
            public string fileName;
            public ErrorRec()
            {
                errorCode = ErrCodes.EcOk;
                errorLine = 0;
                fileName = "";
            }
            public ErrorRec(ErrCodes code, int line, string file)
            {
                errorCode = code;
                errorLine = line;
                fileName = file;
            }   
            public static string ErrorTextByCode( ErrCodes code )
            {
                switch( code )
                {
                  case ErrCodes.EcOk:
                    return "OK";
                  case ErrCodes.EcErrorInDefineExpression:
                    return "Error In Define Expression";
                  case ErrCodes.EcErrorInIncludeExpressionOrFileNotFound:
                    return "Error In Include Expression Or File Not Found";
                  case ErrCodes.EcErrorIncorrectDirective:
                    return "Error Incorrect Directive";
                  case ErrCodes.EcErrorIdentifierAlreadyDefined:
                    return "Error Identifier Already Defined";
                  case ErrCodes.EcErrorInInitSectionExpression:
                    return "Error in Init Section Expression";
                  case ErrCodes.EcErrorSubDefinePreviouslyDefined:
                    return "Error Subdefine Previously Defined";
                  case ErrCodes.EcErrorInParameters:
                    return "Error Parameters Out Of Range or of Incorrect Type";
                  default:
                    return "Unknown Error";
                }
            }

            public override string ToString() { return ErrorTextByCode(errorCode) + ", in file <" + fileName + ">, at line: "  + errorLine.ToString();}
        };

        private static void AddPreCompiledLines ( ref List<string> funcLines, ref string[] preCompiledLines, ref List<string> dest, ref KeyValList.KeyValList vars   )
        {
            dest.AddRange(preCompiledLines);
            foreach (var t in funcLines)
            {
                var lines = t.Trim();
                var pp = lines.IndexOf(']');
                if (lines.IndexOf('[') == 0 && pp  > 0)
                {
                    lines = lines.Substring(1,pp-1).Trim();
                    RearrangeDefs(ref vars, lines, lines);
                }
            }
        }


        private static ErrorRec DoDef(ref List<string> source, ref List<string> dest, ref KeyValList.KeyValList vars
            , ref int i, ref string trimLine)
        {
            var parsedLine = trimLine.Substring(4).Split('=');
            if (parsedLine.Length < 2)
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, i, "");
            }
            var identifier = parsedLine[0].Trim();
            var value = parsedLine[1].Trim();
            if (CheckIdentifierIsFree(ref vars, identifier))
            {
                RearrangeDefs(ref vars, identifier, value);
                dest.Add(";" + source[i]);
            }
            else
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, i, "");
            }
            return new ErrorRec();
        }


        private static ErrorRec DoMacro(ref List<string> source, ref List<string> dest, ref KeyValList.KeyValList vars
                                    , ref int i, ref string trimLine)
        {
            // TODO: реализовать
            while (trimLine.IndexOf("#endm", StringComparison.Ordinal) != 0)
            {
                i++;
                var line = source[i];
                trimLine = line.Trim();
                if (i > source.Count())
                {
                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, i, "");
                }
            }
            return new ErrorRec();
        }

        private static ErrorRec DoStruct(ref List<string> source, ref List<string> dest, ref KeyValList.KeyValList vars
                                    , ref int i, ref string trimLine, ref string line)
        {

            var parsedLine = trimLine.Substring(7).Split('=' );
            if (parsedLine.Length < 2)
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, i, "");
            }

            var identifierHead = parsedLine[0].Trim();
            var bitCount = 0;
            if (CheckIdentifierIsFree(ref vars, identifierHead))
            {
                if (!CheckPrevDefined(ref vars, identifierHead))
                {
                    var valueHeader = parsedLine[1].Trim();
                    i++;
                    if (i > source.Count)
                    {
                        return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, i, "");
                    }
                    dest.Add(";" + line);
                    line = source[i];
                    trimLine = line.Trim();
                    var structFuncs = new List<string>();
                    structFuncs.Clear();
                    while (trimLine.IndexOf("#ends", StringComparison.Ordinal) != 0)
                    {
                        var fieldSize = 0;
                        parsedLine = trimLine.Split( '='); // equPos = trimLine.IndexOf('=');
                        string identifier;
                        string value;
                        if (parsedLine.Length > 1)
                        {
                            var aStructFieldName = parsedLine[0].Trim();
                            identifier = identifierHead + "." + aStructFieldName;
                            if (!int.TryParse(parsedLine[1].Trim(), out fieldSize))
                            {
                                fieldSize = -1;
                            }
                            if (fieldSize < 1 || fieldSize > 31)
                            {
                                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInParameters, i, "");
                            }
                            structFuncs.Add(";---------------------------");
                            value = $"[_set_{identifierHead}_{aStructFieldName}]";

                            structFuncs.Add(value);
                            structFuncs.Add(";R0 - value");//  ;$VALUE - value, or use as parameter
                            int bitFieldMask = 1 << fieldSize;
                            bitFieldMask--;
                            bitFieldMask <<= bitCount;
                            value = bitCount > 0 
                                ? $"{identifierHead}=&0x{(~bitFieldMask):X}, RA=<<0x{bitCount:X}, RA=&0x{bitFieldMask:X}, {identifierHead}=|RA" 
                                : $"{identifierHead}=&0x{(~bitFieldMask):X}, R0=&0x{bitFieldMask:X}, {identifierHead}=|R0";
                            structFuncs.Add(value);
                            structFuncs.Add(";---------------------------");
                            value = $"[_get_{identifierHead}_{aStructFieldName}]";
                            structFuncs.Add(value);
                            structFuncs.Add(";R0 - result");//  ;$VALUE - result
                            value = bitCount > 0 
                                ? $"R0={identifierHead}, R0=&0x{bitFieldMask:X}, R0=>>0x{bitCount:X}" 
                                : $"R0={identifierHead}, R0=&0x{bitFieldMask:X}";
                            structFuncs.Add(value);
                            value = $"[{bitCount}]";
                            bitCount += fieldSize;
                        }
                        else
                        {
                            identifier = identifierHead + "." + trimLine;
                            value = $"[{bitCount}]";
                            bitCount++;
                        }
                        if (CheckIdentifierIsFree(ref vars, identifier))
                        {

                            RearrangeDefs(ref vars, identifier, valueHeader + value);
                            dest.Add(";" + line);
                        }
                        else
                        {
                            return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, i, "");
                        }
                        for (int bc = 0; bc < fieldSize; bc++)
                        {
//                            value = System.String.Format("[%u]", bitCount - fieldSize + bc);
                            value = $"[{bitCount - fieldSize + bc}]";
                            var ident = identifier + $"[{bc}]";// + Convert.ToString(bc) + "]";
                            if (CheckIdentifierIsFree(ref vars, ident))
                            {
                                RearrangeDefs(ref vars, ident, valueHeader + value);
                                dest.Add(";" + line);
                            }

                        }
                        i++;
                        line = source[i];
                        trimLine = line.Trim();
                        if (i > source.Count)
                        {
                            return new ErrorRec(ErrorRec.ErrCodes.EcErrorInDefineExpression, i, "");
                        }
                    }
                    RearrangeDefs(ref vars, identifierHead, valueHeader);
                    dest.Add(";" + line);
                    var result = Compile(ref structFuncs, out var strFuncAdd, ref vars);
                    if (result != null && result.errorCode != ErrorRec.ErrCodes.EcOk )
                    {
                        return result;
                    }
                    AddPreCompiledLines(ref structFuncs, ref strFuncAdd, ref dest, ref vars);
                    structFuncs.Clear();
                }
                else
                {
                    return new ErrorRec(ErrorRec.ErrCodes.EcErrorSubDefinePreviouslyDefined, i, "");
                }
            }
            else
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined, i, "");
            }
            return new ErrorRec();
        }

        private static ErrorRec DoInclude(ref List<string> dest, ref KeyValList.KeyValList vars
                            , ref int i, ref string trimLine)
        {
            var parsedLine = trimLine.Split( '=');
            var includeFileName = parsedLine[1].Trim();
            if (System.IO.File.Exists(includeFileName))
            {
                var include = System.IO.File.ReadAllLines(includeFileName).ToList();
                var result = Compile(ref include, out var dst, ref vars);                           
                if (result != null && result.errorCode != ErrorRec.ErrCodes.EcOk)
                {
                    if (string.IsNullOrEmpty(result.fileName))
                    {
                        result.fileName = includeFileName;
                    }
                    return result;
                }
                AddPreCompiledLines(ref include, ref dst, ref dest, ref vars);
                include.Clear();
            }
            else
            {
                return new ErrorRec(ErrorRec.ErrCodes.EcErrorInIncludeExpressionOrFileNotFound, i, "");
            }
            return new ErrorRec();
        }

        public static void InitPredefinedVars(ref KeyValList.KeyValList vars)
        {
            RearrangeDefs(ref vars, "$OPERATION", "RE");     // Код операции.
            RearrangeDefs(ref vars, "$OPERATION.READ", "1");  // Значения кодов операции (USER - пользовательская операция)
            RearrangeDefs(ref vars, "$OPERATION.VERIFY", "2");
            RearrangeDefs(ref vars, "$OPERATION.WRITE", "3");
            RearrangeDefs(ref vars, "$OPERATION.USER", "4");

            RearrangeDefs(ref vars, "$VALUE", "RA");          // Используется как параметр функций, и как результат комманды PRINT=A("..")
            RearrangeDefs(ref vars, "$VALUE.Ok", "1");        //    Варианты результата PRINT=A()
            RearrangeDefs(ref vars, "$VALUE.Cancel");

            RearrangeDefs(ref vars, "#type.bin", "B");        // При описании интерфейсной части, символические имена типов отображения значений регистров
            RearrangeDefs(ref vars, "#type.check_box", "C");
            RearrangeDefs(ref vars, "#type.dec", "D");
            RearrangeDefs(ref vars, "#type.hex_editor", "E");
            RearrangeDefs(ref vars, "#type.hex", "H");
            RearrangeDefs(ref vars, "#type.list", "L");
            RearrangeDefs(ref vars, "#type.string", "S");
            RearrangeDefs(ref vars, "#type.push_button", "P");
        }

        public static bool FuncDefCheck(string line, ref KeyValList.KeyValList vars)
        {
            var m = Regex.Match(line, SectionRegex);
            if (m.Success)
            {
                var identifier = m.Groups[1].Value;
                if (vars.IndexOfKey(identifier) < 0)
                {
                    RearrangeDefs(ref vars, identifier, identifier);
                }
                return true;
            }
            return false;
        }

        public static ErrorRec Compile(ref List<string> source, out string[] dest, ref KeyValList.KeyValList vars) 
        {
            if (vars.Count == 0) InitPredefinedVars(ref vars);
            var destBuf = new List<string>();
            for (var i = 0; i < source.Count; i++)
            {
                var line = source[i];
                if (FuncDefCheck(line, ref vars))
                {
                    destBuf.Add(line);
                    continue;
                }
                var trimLine = line.Trim();
                var commentPos = trimLine.IndexOf(';');
                if (commentPos >= 0) trimLine = trimLine.Substring(0, commentPos);
                int equPos;
                dest = null;
                if (trimLine.Length > 0 && trimLine[0] == '#')
                {
                    if (trimLine.IndexOf("#def", StringComparison.Ordinal) == 0)
                    {
                        var r = DoDef(ref source, ref destBuf, ref vars, ref i, ref trimLine);
                        if (r.errorCode != ErrorRec.ErrCodes.EcOk)
                        {
                            return r;
                        }
                    }                
                    else if (trimLine.IndexOf("#struct", StringComparison.Ordinal) == 0)
                    {
                        var r = DoStruct(ref source, ref destBuf, ref vars, ref i, ref trimLine, ref line);
                        if (r.errorCode != ErrorRec.ErrCodes.EcOk)
                        {
                            return r;
                        }
                    }
                    else if (trimLine.IndexOf("#macro", StringComparison.Ordinal) == 0)
                    {
                        var r = DoMacro(ref source, ref destBuf, ref vars, ref i, ref trimLine);
                        if (r.errorCode != ErrorRec.ErrCodes.EcOk)
                        {
                            return r;
                        }

                    }
                    else if(trimLine.IndexOf("#include", StringComparison.Ordinal) == 0)
                    {
                        var r = DoInclude(ref destBuf, ref vars, ref i, ref trimLine);
                        if (r.errorCode != ErrorRec.ErrCodes.EcOk)
                        {
                            return r;
                        }

                    }
                    else
                    {
                        return new ErrorRec(ErrorRec.ErrCodes.EcErrorIncorrectDirective, i, "");
                    }
                }
                else
                {
                    string identifier;
                    string value;
                    if (trimLine.IndexOf("PIN", StringComparison.Ordinal) == 0)
                    {
                        // PINO = CLK,12,8
                        var pinEndPos = 3;
                        if (trimLine.IndexOf("PINI", StringComparison.Ordinal) == 0 || trimLine.IndexOf("PINO", StringComparison.Ordinal) == 0 || trimLine.IndexOf("PING", StringComparison.Ordinal) == 0)
                        {
                            pinEndPos = 4;
                        }

                        identifier = trimLine.Substring(pinEndPos).Trim();
                        if (identifier.IndexOf('=') == 0)
                        {
                            equPos = identifier.IndexOf(',');
                            if (equPos > 1)
                            {
                                value = identifier.Substring(1, equPos - 1).Trim();
                                RearrangeDefs(ref vars, value, value);
                                destBuf.Add(line);
                                continue;
                            }
                            return new ErrorRec(ErrorRec.ErrCodes.EcErrorInInitSectionExpression, i, "");
                        }
                        return new ErrorRec( ErrorRec.ErrCodes.EcErrorInInitSectionExpression, i,"" );
                    }
                    if (trimLine.IndexOf("BUS", StringComparison.Ordinal) == 0)
                    {
                        int pinEndPos = 3;
                        if (trimLine.IndexOf("BUSI", StringComparison.Ordinal) == 0 || trimLine.IndexOf("BUSO", StringComparison.Ordinal) == 0)
                        {
                            pinEndPos = 4;
                        }

                        identifier = trimLine.Substring(pinEndPos).Trim();
                        if (identifier.IndexOf('=') == 0)
                        {
                            equPos = identifier.IndexOf(',');
                            if (equPos > 1)
                            {
                                value = identifier.Substring(1, equPos - 1).Trim();
                                RearrangeDefs(ref vars, value, value);
                                destBuf.Add(line);
                                continue;
                            }
                            return new ErrorRec(ErrorRec.ErrCodes.EcErrorInInitSectionExpression, i, "");
                        }
                        return new ErrorRec(ErrorRec.ErrCodes.EcErrorInInitSectionExpression, i, "");
                    }

                    var needToTranslate = true;
                    if (line.Length > 0 && line[0] != ';')
                    {
                        var noComment = line.Split(';');
                        while (needToTranslate)
                        {
                            needToTranslate = false;
                            for (var ii = 0; ii < vars.Count; ii++)
                            {
                                if (noComment[0].IndexOf(vars[ii], StringComparison.Ordinal) >= 0)
                                {
                                    needToTranslate = vars[ii] != vars[vars[ii]];
                                    noComment[0] = noComment[0].Replace(vars[ii], vars[vars[ii]]);
                                }
                            }
                        }
                        line = "";
//                        if (noComment.Length > 1)
                        {
                            foreach (string part in noComment)
                                if (line != "")
                                    line = line + ";" + part;
                                else
                                    line = part;
                        }
                    }
                    destBuf.Add(line);
                }

            }

            dest = destBuf.ToArray();
            return new ErrorRec();
        }
        //-----------------------
        public static string[] keywords = {"HPL", "ADDR", "ALLPINS", "ALTNAME", "BCOLOR", "BGCOLOR", "BREAK", "BUSI", "BUSO"
                                             ,"CONST", "CDELAY", "DATA", "EXIT", "GAP", "GET", "IENABLE", "IDISABLE"
                                             , "INFO", "INFOFILE", "IMAGE", "LOOP", "MARK", "MATRIX", "OPTIONS", "PING", "PINI", "PINO"
                                             , "PIN", "PLACE", "POWER", "PRINT", "SHAPE", "SIZE", "SPACE", "TCOLOR"
                                             , "TR", "TL", "TD", "TU", "D", "C", "L", "R", "A", "S", "E", "N", "O"
                                             , "REG", "RETURN", "SOCKET", "VCC", "VPP"};

        public static string[] registers = {"$SIZE", "$MAXSIZE", "$FREQ", "$CHNS", "$AREA", "$BLOCKSIZE", "$CDELAY", "$WDELAY"
                                             , "$VERIFY", "$VERSION", "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8"
                                             , "R9", "RA", "RB", "RC", "RD", "RE", "RF", "R10", "R11", "R12", "R13", "R14"
                                             , "R15", "R16", "R17", "R18", "R19", "R1A", "R1B", "R1C", "R1D", "R1E", "R1F"};
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
       public static bool RearrangeDefs(ref KeyValList.KeyValList vars, string identifier, string value = "0")
        {
            
            var foundKeys = new List<string>(); 
            //var foundVals = new List<string>();
            for (var i = 0; i < vars.Count; i++)
            {
                if (identifier.IndexOf(vars[i], StringComparison.Ordinal) >= 0)
                {
                    vars.Insert(i,identifier,value);
                    return true;
                }

            }
            vars[identifier] = value;
            return false;
        }    
    }
}
 