using Autofac;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Services {

    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenanceBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly ShellSettings _shellSettings;

        public MaintenanceBanner(ShellSettings shellSettings, IOrchardServices orchardServices, IShellSettingsManager shellSettingsManager, IOrchardHost orchardHost) {
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var listofcontentitems = _orchardServices.ContentManager.Query<MaintenancePart>(VersionOptions.Published).List();
            foreach (var y in listofcontentitems) {
                yield return new NotifyEntry { Message = T(y.As<MaintenancePart>().MaintenanceNotify), Type = y.As<MaintenancePart>().MaintenanceNotifyType };
            }

            //// Fetching the shell settings for a tenant. This is more efficient if you have many tenants with the Lombiq Hosting Suite,
            //// see below.
            string currentTenant=_shellSettings.Name;
            if (currentTenant.ToLower() != "default") {
                var tenantShellSettings = _shellSettingsManager.LoadSettings().Where(settings => settings.Name == "Default").Single();
                var shellContext = _orchardHost.GetShellContext(tenantShellSettings);
                if (shellContext != null) {
                    using (var wc = shellContext.LifetimeScope.Resolve<IWorkContextAccessor>().CreateWorkContextScope()) {
                        //     var tenantSiteName = wc.Resolve<ISiteService>().GetSiteSettings().SiteName;
                        List<MaintenanceVM> ListMaintenanceVM = new List<MaintenanceVM>();
                        try {
                            ListMaintenanceVM = wc.Resolve<IMaintenanceService>().Get().Where(y => y.Selected_TenantVM.Contains(currentTenant)).ToList();
                        } catch {
                            // non so a priori se il master (default tenant) ha il modulo maintenance enabled
                        }
                        foreach (var y in ListMaintenanceVM) {
                            yield return new NotifyEntry { Message = T(y.MaintenanceNotify), Type = y.MaintenanceNotifyType };
                        }
                    }
                }
            }
        }
    }
}