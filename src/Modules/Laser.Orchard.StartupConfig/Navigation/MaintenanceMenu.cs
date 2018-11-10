using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Core.Settings;
using Laser.Orchard.StartupConfig.Security;

namespace Laser.Orchard.StartupConfig.Navigation {
   [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]   
public class MaintenanceMenu : INavigationProvider {
       public MaintenanceMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(
                menu => menu
                     .Caption(T("Maintenance"))
                         .Position("1")
                         .Action("Index", "MaintenanceAdmin", new { area = "Laser.Orchard.StartupConfig" })
                         .Permission(MaintenancePermission.Maintenance)
                         );
                    //.Add(T("Maintenance"), "1", item => item.Action("Index", "MaintenanceAdmin", new { area = "Laser.Orchard.StartupConfig.Maintenance" })
                    //.Permission(MaintenancePermission.Maintenance)));
        }
    }
}