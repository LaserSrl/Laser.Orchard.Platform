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
        /// <summary>
        /// SaveIfEmpty = True -> if Required = False and IsVisible = False, Value is saved even if it's empty.
        /// SaveIfEmpty = False -> Value is never saved when empty.
        /// </summary>
        public bool SaveIfEmpty { get; set; }
        /// <summary>
        /// HasValue = True -> there already is a value saved in the Field.
        /// </summary>
        public bool HasValue { get; set; }

        public EncryptedStringFieldEditViewModel(EncryptedStringFieldSettings settings) {
            Settings = settings;
        }
    }
}