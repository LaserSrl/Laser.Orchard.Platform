using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Settings {
    public class HashedStringFieldSettings {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Pattern { get; set; }
        /// <summary>
        /// Show the Confirm input box.
        /// </summary>
        public bool ConfirmRequired { get; set; }
    }
}