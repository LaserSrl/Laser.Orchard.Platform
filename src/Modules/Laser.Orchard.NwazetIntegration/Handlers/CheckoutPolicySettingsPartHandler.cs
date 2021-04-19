using Laser.Orchard.NwazetIntegration.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class CheckoutPolicySettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;

        public CheckoutPolicySettingsPartHandler(
            ISignals signals) {

            _signals = signals;

            Filters.Add(new ActivatingFilter<CheckoutPolicySettingsPart>("Site"));

            // Evict cached content when updated, removed or destroyed.
            OnUpdated<CheckoutPolicySettingsPart>(
                (context, part) => Invalidate());
            OnImported<CheckoutPolicySettingsPart>(
                (context, part) => Invalidate());
            OnPublished<CheckoutPolicySettingsPart>(
                (context, part) => Invalidate());
            OnRemoved<CheckoutPolicySettingsPart>(
                (context, part) => Invalidate());
            OnDestroyed<CheckoutPolicySettingsPart>(
                (context, part) => Invalidate());
        }

        private void Invalidate() {
            _signals.Trigger(CheckoutPolicySettingsPart.CacheKey);
        }
    }
}