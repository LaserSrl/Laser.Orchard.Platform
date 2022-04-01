using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ZoneAlternates.Models {
    public class ContentAlternatePartSettings {
        public string AlternateNames { get; set; }
        public string[] GetAlternates() {
            // AlternateNames might be null.
            // This happens when AlternatePart is added to a Content Type but its Content Definition is never saved after adding the part.
            if (string.IsNullOrWhiteSpace(AlternateNames)) {
                return new string[0];
            }
            return AlternateNames.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}