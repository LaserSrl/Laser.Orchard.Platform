using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class EcommerceInvoiceSettingsPartHandler : ContentHandler {
        public EcommerceInvoiceSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<EcommerceInvoiceSettingsPart>("Site"));
        }
    }
}