using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HPLStudio
{
    class Macro
    {
        public Macro()
        {
        }

        public string Name { get; set; }
        public Regex Match { get; set; }
        public List<Regex> ArgsMatch { get; set; }
        public string Body { get; set; }

        private static Regex MacroDefRe = new Regex(@"^#macro\s+(\w+)(\(([\w,\s]+)\))?",
            RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Возвращает параметры макроса из Match его определения
        /// </summary>
        /// <param name="macrodef">Match от MacroDefRe</param>
        /// <returns>(string Name, Regex MacroMatch, List&lt;REgex&gt; ArgsMatch </returns>
        public static (string, Regex, List<Regex>) GenerateMacroProperties(Match macrodef)
        {//(test\((@?[\w]+),\s?(@?[\w]+),\s?(@?[\w]+)\))
            try
            {
                var name = macrodef.Groups[1].Value;
                if (macrodef.Groups.Count < 3)
                {
                    return (name, new Regex(@$"\W({name})\W", RegexOptions.Singleline | RegexOptions.Compiled), 
                        new List<Regex>(0));
                }

                var args = macrodef.Groups[3].Value.Split(',')
                    .Select(x => new Regex(
                            @$"\W({x.Trim()})\W", 
                            RegexOptions.Singleline | RegexOptions.Compiled)).ToList();

                var defArgs = args.Select( x => @"(@?[\w]+)");
                var defArgStr = string.Join(@"\s*,\s*", defArgs);

                var macroCallMatchStr = $@"\W({name}\(\s*{defArgStr}\s*\))\W";
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


        public string GenerateReplaceCode(Match macroMatch)
        {
            var s = Body;
            var i = 2;
            var r = s;
            foreach (var regex in ArgsMatch)
            {
                r = regex.Replace(r, x => $"{x.Value[0]}{macroMatch.Groups[i].Value}{x.Value.Last()}");
                i++;
            }

            return $"{macroMatch.Value[0]}{r}{macroMatch.Value.Last()}";
        }

        public string Apply(string source)
        {
            return Match.Replace(source, GenerateReplaceCode);
        }

        

    }
}
