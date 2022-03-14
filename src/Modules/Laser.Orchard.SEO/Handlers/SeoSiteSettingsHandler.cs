using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.SEO.Handlers {
    public class SeoSiteSettingsHandler : ContentHandler {

        public SeoSiteSettingsHandler() {
            Filters.Add(new ActivatingFilter<SeoSiteSettingsPart>("Site"));
        }
    }
}