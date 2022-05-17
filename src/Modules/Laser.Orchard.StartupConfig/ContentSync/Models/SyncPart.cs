using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync.Models {
    public class SyncPart : ContentPart<SyncPartRecord> {
        public int SyncronizedRef {
            get { return (Retrieve(x => x.SyncronizedRef)); }
            set { Store(x => x.SyncronizedRef, value); }
        }
    }
}