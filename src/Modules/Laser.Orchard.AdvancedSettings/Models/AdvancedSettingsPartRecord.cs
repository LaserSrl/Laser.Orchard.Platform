using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Models {
    public class AdvancedSettingsPartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
    }
}