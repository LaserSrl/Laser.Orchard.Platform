using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class CheckoutSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public CheckoutSettingsPartHandler(
            ISignals signals) {

            _signals = signals;

            Filters.Add(new ActivatingFilter<CheckoutSettingsPart>("Site"));

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<CheckoutSettingsPart>(
                (context, part) => Invalidate());
            OnImported<CheckoutSettingsPart>(
                (context, part) => Invalidate());
            OnPublished<CheckoutSettingsPart>(
                (context, part) => Invalidate());
            OnRemoved<CheckoutSettingsPart>(
                (context, part) => Invalidate());
            OnDestroyed<CheckoutSettingsPart>(
                (context, part) => Invalidate());
        }

        private void Invalidate() {
            _signals.Trigger(Constants.CheckoutSettingsCacheEvictSignal);
        }
    }
}