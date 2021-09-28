using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ZoneAlternates.Models {
    public class ContentAlternatePartSettings {
        public string AlternateNames { get; set; }
        public string[] GetAlternates() {
            return AlternateNames.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}