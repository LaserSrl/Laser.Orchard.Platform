using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.PaymentGateway.Handlers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosSiteSettingsPartHandler : ContentHandler {
        public CustomPosSiteSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Filters.Add(new ActivatingFilter<CustomPosSiteSettingsPart>("Site"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType!="Site") {
                return;
            }
            base.GetItemMetadata(context);
        }
    }
}