
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CountriesHierarchyDoesNotExist : INotificationProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IAddressConfigurationSettingsService _addressConfigurationSettingsService;

        public CountriesHierarchyDoesNotExist(
            IWorkContextAccessor workContextAccessor,
            IAddressConfigurationSettingsService addressConfigurationSettingsService) {

            _workContextAccessor = workContextAccessor;
            _addressConfigurationSettingsService = addressConfigurationSettingsService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var hierarchy = _addressConfigurationSettingsService.ShippingCountriesHierarchy;
            if (hierarchy == null) {
                yield return new NotifyEntry {
                    Message = T("No countries' hierarchy was configured. Have an administrator configure one <a href=\"{0}\">here</a>.", _settingsUrl),
                    Type = NotifyType.Error
                };
            } else if (!_addressConfigurationSettingsService.SelectedCountryIds.Any()) {
                yield return new NotifyEntry {
                    Message = T("No valid countries were configured. Have an administrator configure one <a href=\"{0}\">here</a>.", _settingsUrl),
                    Type = NotifyType.Error
                };
            }
        }

        private string _settingsUrl {
            get {
                var workContext = _workContextAccessor.GetContext();
                return new UrlHelper(workContext.HttpContext.Request.RequestContext)
                    .Action("Index", "ECommerceSettingsAdmin", new { Area = "Nwazet.Commerce" });
            }
        }
    }
}
