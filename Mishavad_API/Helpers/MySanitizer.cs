using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Text.RegularExpressions;

namespace Mishavad_API.Helpers
{
    public class MySanitizer
    {
        public static string StrictSanitize(string input, string[] allowed_tags = null) {

            StringBuilder sb = new StringBuilder(
                            HttpUtility.HtmlEncode(input));

            // allowed_tags = { "b", "i", "p","strong" };
            if (allowed_tags != null)
            {
                foreach (var _tag in allowed_tags)
                {
                    sb.Replace("&lt;" + _tag + "&gt;", "");
                    sb.Replace("&lt;/" + _tag + "&gt;", "");
                }
            }

            var rgx = new Regex("&lt;.*&gt;");
            string result = rgx.Replace(sb.ToString(), "");

            return result;
        }
    }
}