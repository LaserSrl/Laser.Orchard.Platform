using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Faq
{
    public static class Helper
    {
        public static String GetTrimmedString(this string str, int countOfSymbols)
        {
            string result = string.Empty;
            if (!String.IsNullOrEmpty(str))
            {
                if (str.Length > countOfSymbols)
                {
                    result = str.Substring(0, countOfSymbols).TrimEnd() + " ...";
                }
                else
                {
                    result = str;
                }
            }
            return result;
        }
    }
}