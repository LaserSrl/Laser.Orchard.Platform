using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Maps.Handlers {
    public class MapsSiteSettingsHandler : ContentHandler {

        public MapsSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            Filters.Add(new ActivatingFilter<MapsSiteSettingsPart>("Site"));
            OnInitializing<MapsSiteSettingsPart>((context, part) => {
                part.MapsProvider = MapsProviders.Google;
            });
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Maps")));
        }

    }
}