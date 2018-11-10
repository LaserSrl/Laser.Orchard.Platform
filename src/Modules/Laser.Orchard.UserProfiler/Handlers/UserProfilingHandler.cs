using Laser.Orchard.UserProfiler.Models;
using Laser.Orchard.UserProfiler.Service;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Handlers {
    public class UserProfilingHandler  : ContentHandler {
        public UserProfilingHandler(IRepository<UserProfilingPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            }
        }
}