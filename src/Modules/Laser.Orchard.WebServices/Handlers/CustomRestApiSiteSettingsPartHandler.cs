using Laser.Orchard.WebServices.Helpers;
using Laser.Orchard.WebServices.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.WebServices.Handlers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class CustomRestApiSiteSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;
        
        public CustomRestApiSiteSettingsPartHandler(
            ISignals signals) {

            _signals = signals;

            Filters.Add(new ActivatingFilter<CustomRestApiSiteSettingsPart>("Site"));

            OnRemoved<CustomRestApiSiteSettingsPart>((context, part) => EvictCache(part));
            OnImported<CustomRestApiSiteSettingsPart>((context, part) => EvictCache(part));
            OnUpdated<CustomRestApiSiteSettingsPart>((context, part) => EvictCache(part));
            OnPublished<CustomRestApiSiteSettingsPart>((context, part) => EvictCache(part));
        }

        private void EvictCache(CustomRestApiSiteSettingsPart part) {
            // Settings part is cached to avoid expensive trips to the db on every call
            _signals.Trigger(CustomRestApiHelper.SettingsCacheKey);
        }
    }
}