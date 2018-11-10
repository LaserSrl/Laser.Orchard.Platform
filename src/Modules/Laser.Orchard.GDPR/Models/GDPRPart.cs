using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Models {
    /// <summary>
    /// This is the part used to mark ContentItems that should be subject to special processing to
    /// ensure a tenant's compliance with GDPR
    /// </summary>
    public class GDPRPart : ContentPart<GDPRPartRecord> {

        /// <summary>
        /// Tells whether the ContentItem this GDPRPart is attached to should be protected,
        /// meaning that anonymization and erasure should be prevented on this ContentItem
        /// </summary>
        public bool IsProtected {
            get { return Retrieve(r => r.IsProtected); }
            set { Store(r => r.IsProtected, value); }
        }
    }
}