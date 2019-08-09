using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePart : ContentPart<PerItemCachePartRecord> {
        public string PerItemKeyParam {
            get { return Retrieve(r => r.PerItemKeyParam); }
            set { Store(r => r.PerItemKeyParam, value); }
        }
    }
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePartRecord : ContentPartVersionRecord {
        public virtual string PerItemKeyParam { get; set; }
    }
}