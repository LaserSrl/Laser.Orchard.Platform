using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.PaymentGateway.Handlers {
    public class PaymentGatewaySiteSettingsPartHandler : ContentHandler {
        public PaymentGatewaySiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<PaymentGatewaySiteSettingsPart>("Site"));
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") {
                return;
            }
            base.GetItemMetadata(context);
        }
    }
}