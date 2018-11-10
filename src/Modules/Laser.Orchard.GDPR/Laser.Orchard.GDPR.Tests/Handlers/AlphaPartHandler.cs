using Laser.Orchard.GDPR.Tests.Models;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.GDPR.Tests.Handlers {
    public class AlphaPartHandler : ContentHandler {
        public AlphaPartHandler() {
            //Filters.Add(new ActivatingFilter<AlphaPart>("NoGDPRType"));
        }
    }
}
