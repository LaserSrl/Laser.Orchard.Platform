using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.AuditTrail.ViewModels {
    public class AuditTrailOutputSettingsPartViewModel {
        public bool IsEventViewerEnabled { get; set; }
        public string EventViewerSourceName { get; set; }

        public bool IsFileSystemEnabled { get; set; }
    }
}