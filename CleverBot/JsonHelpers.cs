using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cleverbot
{
    class JsonHelpers
    {
        public static string Property(string name, string value)
        {
            return String.Format("\"{0}\":\"{1}\",", name, value);
        }

        public static string Segment(string text)
        {
            if (text.EndsWith(","))
            {
                text = text.Remove(text.LastIndexOf(','));
            }
            return "{" + text + "}";
        }

        public static Dictionary<string, string> jsonToDictionary(string text)
        {
            text = text.Remove(0, 1);
            text = text.Remove(text.Length - 1, 1);
            Dictionary<string, string> output = new Dictionary<string, string>();
            if (text.Contains(":") || text.Contains(","))
            {
                var arr = Regex.Matches(text, "(\"[\\w]+?)\":(\"| \")([\\w].+?)\"");
                foreach (Match param in arr)
                {
                    string[] splitted = param.Value.Split(':');
                    output.Add(splitted[0].Replace("\"", ""), splitted[1].Replace("\"", ""));
                }
                return output;
            }
            else
                return null;

        }
    }
}
