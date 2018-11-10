using Laser.Orchard.UserProfiler.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Handlers {
    public class TrackingPartHandler  : ContentHandler {
        public TrackingPartHandler(IRepository<TrackingPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            }
        }
}