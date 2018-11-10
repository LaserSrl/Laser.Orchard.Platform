using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Extensions {
    public static class DictionaryExtension {
        public static string ToJson(this IDictionary<string, string> dic) {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool insertComma = false;
            foreach (var key in dic.Keys) {
                if (insertComma) {
                    sb.Append(",");
                }
                else {
                    insertComma = true;
                }
                sb.AppendFormat("\"{0}\":\"{1}\"", FormatJsonValue(key), FormatJsonValue(dic[key]));
            }
            sb.Append("}");

            return sb.ToString();
        }

        private static string FormatJsonValue(string text) {
            return (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}