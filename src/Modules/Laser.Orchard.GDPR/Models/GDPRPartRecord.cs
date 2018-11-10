using Orchard.ContentManagement.Records;

namespace Laser.Orchard.GDPR.Models {
    public class GDPRPartRecord : ContentPartRecord {
        public virtual bool IsProtected { get; set; }
    }
}