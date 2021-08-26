using Laser.Orchard.Cookies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Cookies.Handlers {
    [OrchardFeature("Laser.Orchard.HubSpot")]
    public class HubSpotSettingPartHandler : ContentHandler {
        public HubSpotSettingPartHandler()
        {
            Filters.Add(new ActivatingFilter<HubSpotSettingsPart>("Site"));
        }

    }
}