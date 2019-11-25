using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Cookies.Handlers {
    public class HubSpotSettingPartHandler : ContentHandler {
        public HubSpotSettingPartHandler()
        {
            Filters.Add(new ActivatingFilter<HubSpotSettingsPart>("Site"));
        }

    }
}