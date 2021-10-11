using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Reporting.Models {
    public class ColumnParser {

        public ColumnParser(string title) {
            // parse received title
            if (title.StartsWith("ahref", StringComparison.InvariantCultureIgnoreCase)) {
                var options = title
                    .Substring(5)
                    .TrimStart('(').TrimEnd(')')
                    .Split(new char[] { ';' }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .ToArray();
                Alias = options[0];
                Action = (r, obj) => {
                    r.Add(new ParsedColumn() {
                        Alias = options[0],
                        Value = obj,
                        ColumnType = ColumnType.AHREF,
                        Options = options.Skip(1).ToList<object>()
                    });
                };
            } else if (title.StartsWith("imgsrc", StringComparison.InvariantCultureIgnoreCase)) {
                var options = title
                    .Substring(6)
                    .TrimStart('(').TrimEnd(')')
                    .Split(new char[] { ';' }, StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .ToArray();
                Alias = options[0];
                Action = (r, obj) => {
                    r.Add(new ParsedColumn() {
                        Alias = options[0],
                        Value = obj,
                        ColumnType = ColumnType.IMGSRC,
                        Options = options.Skip(1).ToList<object>()
                    });
                };
            } else {
                Alias = title;
                Action = (r, obj) => r.Add(obj);
            }
        }

        public string Alias { get; set; }

        public Action<List<object>, object> Action { get; set; }
    }

    public class ParsedColumn {
        public ParsedColumn() {
            Options = new List<object>();
        }
        public string Alias { get; set; }
        public object Value { get; set; }
        public ColumnType ColumnType { get; set; }
        public List<object> Options { get; set; }

        public string ParseValue() {
            if (Value == null) {
                return "null";
            }
            var text = Options.FirstOrDefault()?.ToString();
            switch (ColumnType) {
                case ColumnType.AHREF:
                    return string.Format(@"<a href='{0}'>{1}</a>",
                            Value.ToString(),
                            string.IsNullOrWhiteSpace(text) ? Alias : text)
                        .Replace("\n", "\\r\\n")
                        .Replace(Environment.NewLine, "\\r\\n")
                        .Replace("\"", "\\\"");
                case ColumnType.IMGSRC:
                    return string.Format(@"<img src='{0}' title={1} />",
                            Value.ToString(),
                            string.IsNullOrWhiteSpace(text) ? Alias : text)
                        .Replace("\n", "\\r\\n")
                        .Replace(Environment.NewLine, "\\r\\n")
                        .Replace("\"", "\\\"");
                case ColumnType.None:
                default:
                    return Value.ToString()
                        .Replace("\n", "\\r\\n")
                        .Replace(Environment.NewLine, "\\r\\n")
                        .Replace("\"", "\\\"");
            }
        }
    }

    public enum ColumnType {
        None,
        AHREF,
        IMGSRC
    }
}