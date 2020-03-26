
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Laser.Orchard.GoogleAnalytics.Models;
using Orchard.Caching;

namespace Laser.Orchard.GoogleAnalytics.Handlers {
	
	[OrchardFeature("Laser.Orchard.GoogleAnalytics")]
	public class GoogleAnalyticsSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;
        public GoogleAnalyticsSettingsPartHandler(
            IRepository<GoogleAnalyticsSettingsPartRecord> repository,
            ISignals signals) {

            _signals = signals;

            Filters.Add(new ActivatingFilter<GoogleAnalyticsSettingsPart>("Site"));
			Filters.Add(StorageFilter.For(repository));

            OnUpdated<GoogleAnalyticsSettingsPart>((ctx, part) => {
                _signals.Trigger(Constants.SiteSettingsEvictSignal);
            });
		}
	}
}