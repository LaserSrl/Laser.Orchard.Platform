using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Extensions {
    /// <summary>
    /// Masks a string to avoid its view.
    /// E.g.: andrea@test.com becomes and*****com.
    /// </summary>
    /// <param name="strToMask"></param>
    /// <param name="percentage">Percentage of the string to mask.</param>
    /// <returns></returns>
    public static class StringExtensions {
        public static string ToMaskedString(this string strToMask, decimal percentage = 70) {
            string first, last;

            if (strToMask.Length > 10) {
                first = strToMask.Substring(0, (int)((strToMask.Length - (strToMask.Length * (percentage / 100))) / 2));
                last = strToMask.Substring(strToMask.Length - (int)((strToMask.Length - (strToMask.Length * (percentage / 100))) / 2));
            } else {
                first = strToMask.Substring(0, 1);
                last = strToMask.Substring(strToMask.Length - 1);
            }
            var maskedString = first + "*****" + last;

            return maskedString;
        }
    }
}