using Laser.Orchard.StartupConfig.ContentSync.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync.Handlers {
    public class SyncPartHandler : ContentHandler {
        public SyncPartHandler(IRepository<SyncPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }

}