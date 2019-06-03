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
    public class OpenRouteSiteSettingsHandler : ContentHandler {

        public OpenRouteSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            Filters.Add(new ActivatingFilter<OpenRouteSiteSettingsPart>("Site"));
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("OpenRouteService")));
        }

    }
}