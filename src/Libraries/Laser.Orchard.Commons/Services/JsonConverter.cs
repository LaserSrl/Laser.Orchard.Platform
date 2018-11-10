using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.Commons.Services {
    public class JsonConverter {
        public static void ConvertToJSon(XElement x, StringBuilder sb, bool minified = false, bool realformat = false) {
            if (x == null) {
                return;
            }

            switch (x.Name.ToString()) {
                case "ul":
                    var first = true;
                    foreach (var li in x.Elements()) {
                        if (!first) sb.Append(",");
                        ConvertToJSon(li, sb, minified, realformat);
                        first = false;
                    }
                    break;
                case "li":
                    var name = x.Element("h1").Value;
                    var value = x.Element("span").Value;
                    string attribute = "";
                    if (x.Element("span").Attribute("type") != null)
                        attribute = x.Element("span").Attribute("type").Value;
                    var ul = x.Element("ul");

                    if (minified) {

                        if ((ul != null && ul.Descendants().Any())) {//&& ul.Descendants().Any()
                            sb.AppendFormat("\"{0}\":[", FormatJsonValue(name));
                            first = true;
                            foreach (var li in ul.Elements()) {
                                sb.Append(first ? "{" : ",{");
                                ConvertToJSon(li, sb, minified, realformat);
                                sb.Append("}");
                                first = false;
                            }
                            sb.Append("]");

                        } else {
                            if (value.StartsWith("List<")) {
                                sb.AppendFormat("\"{0}\":[]", FormatJsonValue(name));
                            } else
                                if (value == "string[]" || value == "Int32[]" || value == "DateTime[]") {
                                    sb.AppendFormat(string.Format("\"{0}\":\"{1}\"", FormatJsonValue(name), FormatJsonValue(value, realformat)));
                                } else {
                                    sb.AppendFormat(string.Format("\"{0}\":{1}", FormatJsonValue(name), FormatJsonValue(value, realformat)));
                                }
                        }
                    } else {
                        sb.AppendFormat("\"n\":\"{0}\",", FormatJsonValue(name));
                        if (realformat)
                            if (ul != null || attribute == "string") {//|| value.StartsWith("LazyField") || value.StartsWith("SingleChoice") || value.StartsWith("None") || value.StartsWith("List<") || value.StartsWith("Show") || value == "string[]" || value == "Int32[]" || value == "DateTime[]"
                                sb.AppendFormat("\"v\":\"{0}\"", FormatJsonValue(value, realformat));
                            } else {
                                sb.AppendFormat("\"v\":{0}", FormatJsonValue(value, realformat));
                            } else
                            sb.AppendFormat("\"v\":\"{0}\"", FormatJsonValue(value, realformat));


                        if (ul != null && ul.Descendants().Any()) {
                            sb.Append(",\"m\":[");
                            first = true;
                            foreach (var li in ul.Elements()) {
                                sb.Append(first ? "{" : ",{");
                                ConvertToJSon(li, sb, minified, realformat);
                                sb.Append("}");
                                first = false;
                            }
                            sb.Append("]");
                        }
                    }
                    break;
            }
        }

        public static string FormatJsonValue(string value, bool realformat = false) {
            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }
            if (realformat) {
                if (value == "True")
                    return "true";
                else
                    if (value == "False")
                        return "false";
                    else
                        if (value.StartsWith("#") && value.EndsWith("#"))
                            value = "\"" + value + "\"";
                //return "\"" + value.Substring(1, value.Length - 2) + "\"";
                value = value.Replace(@"\", @"\\");
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = "\"" + value.Substring(1, value.Length - 2).Replace("\"", @"\""") + "\"";

                return value.Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
            }
            // replace " by \" in json strings
            return value.Replace(@"\", @"\\").Replace("\"", @"\""").Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
            //return HttpUtility.HtmlEncode(value).Replace(@"\", @"\\").Replace("\"", @"\""").Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
        }

    }
}
