using Laser.Orchard.UserProfiler.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Handlers {

    public class ContactProfilingHandler : ContentHandler {

        public ContactProfilingHandler() {
            Filters.Add(new ActivatingFilter<ContactProfilingPart>("CommunicationContact"));
        }
    }
}