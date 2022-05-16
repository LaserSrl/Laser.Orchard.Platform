using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System.Data;
using Orchard.Environment;
using Orchard.ContentManagement;
using Orchard;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.UI.Notify;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.WebApiProtection {

    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class Migration : DataMigrationImpl, IFeatureEventHandler {
        private readonly IOrchardServices _services;
        private readonly ShellSettings _settings;
        private readonly INotifier _notifier;
        private readonly IUtilsServices _utilsServices;

        public Migration(IOrchardServices services, ShellSettings settings, INotifier notifier, IUtilsServices utilsServices) {
            T = NullLocalizer.Instance;
            _services = services;
            _settings = settings;
            _notifier = notifier;
            _utilsServices = utilsServices;
        }

        public Localizer T { get; set; }

        #region [ IFeatureEventHandler Implementaton ]
        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            var settings = _services.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
            if (String.IsNullOrWhiteSpace(settings.ProtectedEntries)) {
                var defaultProtectedEntries = new string[] {
                    "Laser.Orchard.WebServices.Json.GetByAlias",
                    "Laser.Orchard.WebServices.Signal.Trigger",
                    "Laser.Orchard.Braintree.Paypal.GetClientToken",
                    "Laser.Orchard.Braintree.Paypal.Pay",
                    "Laser.Orchard.WebServices.WebApi.Display",
                    "Laser.Orchard.WebServices.WebApi.Terms"
                };
                settings.ProtectedEntries = String.Join(",", defaultProtectedEntries);
            }
            if (!settings.ExternalApplicationList.ExternalApplications.Any()) {
                var appList = new ExternalApplicationList();
                var apps = new List<ExternalApplication>();
                var api = "";
                var name = "";
                apps.Add(new ExternalApplication {
                    Name = name = String.Format("{0}App", _settings.Name),
                    ApiKey = api = new WebApiUtils().RandomString(22),
                    EnableTimeStampVerification = true,
                    Validity = 10
                });
                appList.ExternalApplications = apps;
                settings.ExternalApplicationList = appList;
                _notifier.Information(T("A default app named \"{0}\" has been created. Its Api Key is {1}.", name, api));
            }
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }
        #endregion [ IFeatureEventHandler Implementaton ]

        #region [ DataMigration Implementation ]
        public int Create() {
            _utilsServices.EnableFeature("Orchard.Caching");
            return 1;
        }
        #endregion [ DataMigration Implementation ]
    }


}