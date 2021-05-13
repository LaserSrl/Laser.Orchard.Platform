using Laser.Orchard.SecureData.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.ViewModels {
    public class HashedStringFieldEditViewModel {
        public string Value { get; set; }
        public string ConfirmValue { get; set; }
        public string DisplayName { get; set; }
        public HashedStringFieldSettings Settings { get; set; }
        /// <summary>
        /// SaveIfEmpty = True -> if Required = False and IsVisible = False, Value is saved even if it's empty.
        /// SaveIfEmpty = False -> Value is never saved when empty.
        /// </summary>
        public bool SaveIfEmpty { get; set; }
        /// <summary>
        /// HasValue = True -> there already is a value saved in the Field.
        /// </summary>
        public bool HasValue { get; set; }
        /// <summary>
        /// ResetField = True -> Value, Salt and HashAlgorithm are set to NULL.
        /// </summary>
        public bool ResetField { get; set; }

        public HashedStringFieldEditViewModel(HashedStringFieldSettings settings) {
            Settings = settings;
        }
    }
}