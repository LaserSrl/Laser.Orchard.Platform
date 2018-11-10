using Laser.Orchard.Braintree.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Handlers
{
    public class BraintreeSiteSettingsHandler : ContentHandler
    {
        public BraintreeSiteSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<BraintreeSiteSettingsPart>("Site"));
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
            {
                return;
            }
            base.GetItemMetadata(context);
        }
    }
}