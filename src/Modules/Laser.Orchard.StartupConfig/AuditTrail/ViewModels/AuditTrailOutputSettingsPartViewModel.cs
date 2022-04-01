using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AuditTrail.ViewModels {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class AuditTrailOutputSettingsPartViewModel {
        public bool IsEventViewerEnabled { get; set; }
        public string EventViewerSourceName { get; set; }

        public bool IsFileSystemEnabled { get; set; }
    }
}