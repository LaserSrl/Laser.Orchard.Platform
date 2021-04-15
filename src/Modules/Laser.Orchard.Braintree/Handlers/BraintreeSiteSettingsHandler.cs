using Laser.Orchard.Braintree.Models;
using Orchard.Caching;
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
    public class BraintreeSiteSettingsHandler : ContentHandler {
        private readonly ISignals _signals;
        public BraintreeSiteSettingsHandler(
            ISignals signals) {

            _signals = signals;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<BraintreeSiteSettingsPart>("Site"));

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<BraintreeSiteSettingsPart>(
                (context, part) => Invalidate());
            OnImported<BraintreeSiteSettingsPart>(
                (context, part) => Invalidate());
            OnPublished<BraintreeSiteSettingsPart>(
                (context, part) => Invalidate());
            OnRemoved<BraintreeSiteSettingsPart>(
                (context, part) => Invalidate());
            OnDestroyed<BraintreeSiteSettingsPart>(
                (context, part) => Invalidate());
        }

        public Localizer T { get; set; }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
            {
                return;
            }
            base.GetItemMetadata(context);
        }
        private void Invalidate() {
            _signals.Trigger(BraintreeSiteSettingsPart.CacheKey);
        }
    }
}