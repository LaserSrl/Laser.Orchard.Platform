using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AuditTrail.Models {
    [OrchardFeature("Laser.Orchard.AuditTrail")]
    public class AuditTrailOutputSettingsPart : ContentPart {

        public virtual bool IsEventViewerEnabled {
            get { return this.Retrieve(p => p.IsEventViewerEnabled); }
            set { this.Store(p => p.IsEventViewerEnabled, value); }
        }
        // TODO: when the feature is enabled, set this to the tenant name
        public virtual string EventViewerSourceName {
            get { return this.Retrieve(p => p.EventViewerSourceName); }
            set { this.Store(p => p.EventViewerSourceName, value); }
        }

        public virtual bool IsFileSystemEnabled {
            get { return this.Retrieve(p => p.IsFileSystemEnabled); }
            set { this.Store(p => p.IsFileSystemEnabled, value); }
        }

    }
}