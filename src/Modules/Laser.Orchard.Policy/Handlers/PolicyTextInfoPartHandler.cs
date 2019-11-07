using Laser.Orchard.Policy.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Policy.Handlers {
    public class PolicyTextInfoPartHandler : ContentHandler {
        private readonly ISignals _signals;
        public PolicyTextInfoPartHandler(
            IRepository<PolicyTextInfoPartRecord> repository,
            ISignals signals) {

            _signals = signals;

            Filters.Add(StorageFilter.For(repository));

            // trigger signals that control caching
            OnPublished<PolicyTextInfoPart>((context, part) => Invalidate(part));
            OnRemoved<PolicyTextInfoPart>((context, part) => Invalidate(part));
            OnDestroyed<PolicyTextInfoPart>((context, part) => Invalidate(part));
        }

        private void Invalidate(PolicyTextInfoPart content) {
            _signals.Trigger($"PolicyTextInfoPart_{content.Id}");
            _signals.Trigger("PolicyTextInfoPart_EvictAll");
        }
    }
}