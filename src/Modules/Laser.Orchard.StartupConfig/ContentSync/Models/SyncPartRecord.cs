using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync.Models {
    public class SyncPartRecord : ContentPartRecord {
        public int SyncronizedRef { get; set; }
    }
}