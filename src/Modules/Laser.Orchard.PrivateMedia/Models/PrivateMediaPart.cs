using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.PrivateMedia.Models {
    public class PrivateMediaPart : ContentPart<PrivateMediaPartRecord> {
        public bool IsPrivate {
            get { return Retrieve(r => r.IsPrivate); }
            set { Store(r => r.IsPrivate, value); }
        }
    }
    public class PrivateMediaPartRecord : ContentPartRecord {
        public virtual bool IsPrivate { get; set; }
    }
}