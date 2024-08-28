using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace HPLStudio
{
    class Macro
    {
        public static Dictionary<string, Macro> GlobalStorage = null;

        public Macro()
        {
            GlobalStorage ??= new Dictionary<string, Macro>();
        }

        public string Name { get; set; }
        public Regex Match { get; set; }
        public List<Regex> ArgsMatch { get; set; }
        public string Body { get; set; }

        private static Regex MacroDefRe = new Regex(@"^#macro\s+(\w+)(\(([\w,\s\{\$\}\#]+)\))?",
            RegexOptions.Multiline | RegexOptions.Compiled);

        private static string ArgPattern = @"(@?[\w\{\$\}\s\#]+)";

        /// <summary>
        /// Возвращает параметры макроса из Match его определения
        /// </summary>
        /// <param name="macrodef">Match от MacroDefRe</param>
        /// <returns>(string Name, Regex MacroMatch, List&lt;Regex&gt; ArgsMatch </returns>
        public static (string, Regex, List<Regex>) GenerateMacroProperties(Match macrodef)
        {//(test\((@?[\w]+),\s?(@?[\w]+),\s?(@?[\w]+)\))
            try
            {
                var name = macrodef.Groups[1].Value;
                if (!macrodef.Groups[2].Success) // macro with no args
                {
                    return (name, new Regex(@$"(\b{name}\b)", RegexOptions.Singleline | RegexOptions.Compiled), 
                        new List<Regex>(0));
                }

                var args = macrodef.Groups[3].Value.Split(',')
                    .Select(x => new Regex(
                            $@"(\b{x.Trim()}\b)",//@$"(^|\W)({x.Trim()})(\W|$)", 
                            RegexOptions.Singleline | RegexOptions.Compiled)).ToList();

                var defArgs = args.Select( x => ArgPattern);
                var defArgStr = string.Join(@"\s*,\s*", defArgs);

                var macroCallMatchStr = @$"(\b{name}\(\s*{defArgStr}\s*\))";
                return (name, new Regex(macroCallMatchStr, RegexOptions.Singleline | RegexOptions.Compiled), args);
            }
            catch
            {
                return (null, null, null);
            }
        }

        public static Macro ParseHeader(string header)
        {
            var macrodef = MacroDefRe.Match(header);
            var (name, match, args) = GenerateMacroProperties(macrodef);
            return new Macro()
            {
                Name = name,
                Match = match,
                ArgsMatch = args
            };
        }

        public string Evaluator(Match x, Match macroMatch, int i )
        {
            return $"{macroMatch.Groups[i].Value}";
        }

        public string GenerateReplaceCode(Match macroMatch)
        {
            var s = Body;
            var i = 2;
            var r = s;
            foreach (var regex in ArgsMatch)
            {
                r = regex.Replace(r, x => Evaluator(x, macroMatch, i));
                i++;
            }

            return r;
            //return $"{macroMatch.Groups[1].Value}{r}{macroMatch.Groups[macroMatch.Groups.Count - 1].Value}";
        }

        public string Apply(string source)
        {

            return Match.Replace(source, GenerateReplaceCode);
        }


        // (?<macroDef>#macro\s*.*?)\n+(?<macroBody>.*?)#endm
        private static readonly Regex MacroRe = new Regex(@"(#macro\s*.*?)\n+(.*?)#endm",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public static (ErrorRec, string) ProcessMacroDefs(string source, KeyValList.KeyValList vars=null, Dictionary<string, Macro> macros = null)
        {
            macros ??= GlobalStorage;
            vars ??= Preprocessor.Variables;

            var error = new ErrorRec();
            var dest = MacroRe.Replace(source, x =>
            {
                if (error.Code != ErrorRec.ErrCodes.EcOk) return x.Value;
                var header = x.Groups[1].Value;
                var macro = ParseHeader(header);
                var body = x.Groups[2].Value;
                macro.Body = body.TrimEnd(null);
                if (macros.ContainsKey(macro.Name) || vars.IndexOfKey(macro.Name) >= 0)
                {
                    error = new ErrorRec(ErrorRec.ErrCodes.EcErrorIdentifierAlreadyDefined,
                            x.Index, "")
                        {Info = macro.Name};
                    return x.Value;
                }
                macros.Add(macro.Name, macro);
                var v = x.Value.Replace(Environment.NewLine, $"{Environment.NewLine};");

                return $";{v}";

            });
            return (error, dest);
        }


    }
}
