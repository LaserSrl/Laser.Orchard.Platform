using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class CurrentContentCacheEvictionHandler : ContentHandler {
        private readonly ISignals _signals;

        public CurrentContentCacheEvictionHandler(
            ISignals signals) {

            _signals = signals;

            // Evict cached content when updated, removed or destroyed.
            OnPublished<IContent>((context, part) => Invalidate(part));
            OnRemoved<IContent>((context, part) => Invalidate(part));
            OnDestroyed<IContent>((context, part) => Invalidate(part));
        }

        private void Invalidate(IContent content) {
            var key = $"CurrentContentAccessor_{content.Id}";
            _signals.Trigger(key);
        }
    }
}