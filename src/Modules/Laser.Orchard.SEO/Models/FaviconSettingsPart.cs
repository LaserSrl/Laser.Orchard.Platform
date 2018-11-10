using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.SEO.Models {

    [OrchardFeature("Laser.Orchard.Favicon")]
    public class FaviconSettingsPart : ContentPart<FaviconSettingsRecord> {

        public string FaviconUrl {
            get { return this.Retrieve(x => x.FaviconUrl); }
            set { this.Store(x => x.FaviconUrl, value); }
        }

    }
}
