using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    public class PerItemCachePart : ContentPart<PerItemCachePartRecord> {
        public string PerItemKeyParam {
            get { return Retrieve(r => r.PerItemKeyParam); }
            set { Store(r => r.PerItemKeyParam, value); }
        }
    }

    public class PerItemCachePartRecord : ContentPartVersionRecord {
        public virtual string PerItemKeyParam { get; set; }
    }
}