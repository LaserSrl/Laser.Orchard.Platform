using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Cache.Models {
    [OrchardFeature("Laser.Orchard.NavigationCache")]
    public class CacheableMenuSettingsPart : ContentPart {
        // By default, on activating the feature, all menus will be cached.
        public bool IsFrontEndCacheable {
            get { return this.Retrieve(p => p.IsFrontEndCacheable, true); }
            set { this.Store(p => p.IsFrontEndCacheable, value); }
        }
    }
}