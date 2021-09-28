﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.AdvancedSettings.Models {
    public class AdvancedSettingsPart : ContentPart<AdvancedSettingsPartRecord>, ITitleAspect {

        [MaxLength(255), Required]
        public string Name {
            get {
                return Retrieve<string>(x => x.Name);
            }
            set {
                Store(x => x.Name, value);
            }
        }

        public string Title => Name;
    }
}