using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Laser.Orchard.DataProtection.Models {
    public class DataRestrictionsPart : ContentPart<DataRestrictionsPartRecord> {
        public IList<DataRestrictionsRecord> Restrictions {
            get {
                return Record.Restrictions;
            }
        }
    }

    public class DataRestrictionsPartRecord : ContentPartRecord {
        public DataRestrictionsPartRecord() {
            Restrictions = new List<DataRestrictionsRecord>();
        }
        public virtual IList<DataRestrictionsRecord> Restrictions { get; set; }
    }
}