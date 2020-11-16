using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Settings {
    public class HiddenStringFieldSettings {
        public bool Tokenized { get; set; } //The token selection is always active. However, token substitution is only active when this is true.
        public string TemplateString { get; set; }
        public bool AutomaticAdjustmentOnEdit { get; set; }

        public HiddenStringFieldSettings() {
            Tokenized = true;
        }
    }
}