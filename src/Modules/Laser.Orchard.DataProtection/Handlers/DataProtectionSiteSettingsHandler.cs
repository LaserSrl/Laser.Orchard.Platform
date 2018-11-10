using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Handlers {
    public class DataProtectionSiteSettingsHandler : ContentHandler {
        public Localizer T { get; set; }
        public DataProtectionSiteSettingsHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<DataProtectionSiteSettings>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("DataProtection")));
        }
    }
}