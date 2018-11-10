using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Laser.Orchard.ShortLinks.Models;

namespace Laser.Orchard.ShortLinks.Handlers {
    public class ShortLinksSettingsHandler : ContentHandler {

        public ShortLinksSettingsHandler() {
            Filters.Add(new ActivatingFilter<ShortLinksSettingsPart>("Site"));            
        }

        
    }
   
}