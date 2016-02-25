using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text.RegularExpressions;
namespace Mishavad_API.Helpers
{
    public static class SlugHelper
    {
            public static string GenerateSlug(string phrase)
            {
                string str = phrase.ToLower();
                // invalid chars           
                str = Regex.Replace(str, @"[^\u0622-\u064A\u067E-\u06CCa-z0-9\s-]", "");
                // convert multiple spaces into one space   
                str = Regex.Replace(str, @"\s+", " ").Trim();
                str = Regex.Replace(str, @"\s", "-"); // hyphens   
                // remove one and two letter words (except in the beginning and end of the title 
                str = Regex.Replace(str, @"-[\u0622-\u064A\u067E-\u06CCa-z]{1,2}-", "-"); 
                return str;
            }
    }
}