using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.Queries.Models {
    public class QueryPickerPart : ContentPart {
        private static readonly char[] separator = new[] { '{', '}', ',' };
        public int[] Ids {
            get {
                return
                    (Retrieve<string>("QueriesIds") ?? "")
                    .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToArray();
            }
            set {
                Store("QueriesIds", "{" + string.Join("},{", value.ToArray()) + "}");
            }
        }
    }
}
