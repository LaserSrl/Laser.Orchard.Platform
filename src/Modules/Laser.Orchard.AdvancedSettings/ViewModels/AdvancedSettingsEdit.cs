using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.ViewModels {
    public class AdvancedSettingsEdit {
        public AdvancedSettingsEdit() {
            EditableName = false;
        }
        [MaxLength(255), Required]
        public string Name { get; set; }
        public bool EditableName { get; set; }
    }
}