using Laser.Orchard.StartupConfig.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class AllowCrossOriginSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public AllowCrossOriginSettingsPartHandler(
            ISignals signals) {

            _signals = signals;

            Filters.Add(new ActivatingFilter<AllowCrossOriginSettingsPart>("Site"));

            OnRemoved<AllowCrossOriginSettingsPart>((context, part) => EvictCache(part));
            OnImported<AllowCrossOriginSettingsPart>((context, part) => EvictCache(part));
            OnUpdated<AllowCrossOriginSettingsPart>((context, part) => EvictCache(part));
            OnPublished<AllowCrossOriginSettingsPart>((context, part) => EvictCache(part));
        }

        private void EvictCache(AllowCrossOriginSettingsPart part) {
            // Settings part is cached to avoid expensive trips to the db on every call
            _signals.Trigger(AllowCrossOriginSettingsPart.SettingsCacheKey);
        }
    }
}