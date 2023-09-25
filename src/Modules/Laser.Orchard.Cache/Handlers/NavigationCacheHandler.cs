using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Cache.Handlers {
    [OrchardFeature("Laser.Orchard.NavigationCache")]
    public class NavigationCacheHandler : ContentHandler {
        private readonly ISignals _signals;

        public NavigationCacheHandler(ISignals signals) {
            _signals = signals;

            // Need to reset the navigation cache (to have the menus rebuilt)
            OnImported<IContent>((context, content) => ResetNavigationCache());
            OnPublished<IContent>((context, content) => ResetNavigationCache());
            OnUnpublished<IContent>((context, content) => ResetNavigationCache());
            OnRemoved<IContent>((context, content) => ResetNavigationCache());
            OnDestroyed<IContent>((context, content) => ResetNavigationCache());
        }

        public void ResetNavigationCache() {
            _signals.Trigger("NavigationContentItems.Changed");
        }
    }
}