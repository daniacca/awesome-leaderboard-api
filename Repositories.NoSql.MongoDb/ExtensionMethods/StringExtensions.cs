using System.Text.RegularExpressions;

namespace NoSql.MongoDb.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            var patternTail = new Regex(@"^([A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+)");
            var patternSeparator = new Regex(@"(?<=.+)(_|\-)+(\d{1}|\w{1})");

            string step1 = patternTail.Replace(s, delegate (Match m)
            {
                return m.Value.ToLower();
            });

            string step2 = patternSeparator.Replace(step1, delegate (Match m)
            {
                var match = m.Value;
                return match.Replace("-", "").Replace("_", "");
            });

            return step2;
        }
    }
}
