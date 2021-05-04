using Laser.Orchard.SecureData.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.ViewModels {
    public class EncryptedStringFieldEditViewModel {
        public string Value { get; set; }
        public string ConfirmValue { get; set; }
        public string DisplayName { get; set; }
        public EncryptedStringFieldSettings Settings { get; set; }
    }
}