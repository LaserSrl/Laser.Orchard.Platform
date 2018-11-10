
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Laser.Orchard.GoogleAnalytics.Models;

namespace Laser.Orchard.GoogleAnalytics.Handlers {
	
	[OrchardFeature("Laser.Orchard.GoogleAnalytics")]
	public class GoogleAnalyticsSettingsPartHandler : ContentHandler {
		public GoogleAnalyticsSettingsPartHandler(IRepository<GoogleAnalyticsSettingsPartRecord> repository) {
			Filters.Add(new ActivatingFilter<GoogleAnalyticsSettingsPart>("Site"));
			Filters.Add(StorageFilter.For(repository));
		}
	}
}