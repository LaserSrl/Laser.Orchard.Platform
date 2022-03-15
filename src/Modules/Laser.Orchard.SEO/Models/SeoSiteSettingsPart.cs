using Orchard.ContentManagement;

namespace Laser.Orchard.SEO.Models {
    public class SeoSiteSettingsPart : ContentPart {

        public string CanonicalUrl {
            get { return this.Retrieve(x => x.CanonicalUrl); }
            set { this.Store(x => x.CanonicalUrl, value); }
        }
    }
}