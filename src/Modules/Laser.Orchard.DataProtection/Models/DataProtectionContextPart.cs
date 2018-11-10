using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.DataProtection.Models {
    public class DataProtectionContextPart : ContentPart<DataProtectionContextPartRecord> {
        public string Context {
            get { return Retrieve(r => r.Context); }
            set { Store(r => r.Context, value); }
        }
    }

    public class DataProtectionContextPartRecord : ContentPartVersionRecord {
        public virtual string Context { get; set; }
    }
}