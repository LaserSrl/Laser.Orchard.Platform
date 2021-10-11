using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class EcommerceAnalyticsSettingsPartHandler : ContentHandler {
        public EcommerceAnalyticsSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<EcommerceAnalyticsSettingsPart>("Site"));
        }
    }
}