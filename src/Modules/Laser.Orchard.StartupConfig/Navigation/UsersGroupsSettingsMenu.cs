using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions;
using Orchard.Core.Settings;

namespace Laser.Orchard.StartupConfig.Navigation {
          [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsSettingsProvider : INavigationProvider {
        public UsersGroupsSettingsProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),
                menu => menu.Add(T("Users Groups"), "1", item => item.Action("Settings", "UsersGroupsSettings", new { area = "Laser.Orchard.StartupConfig" })
                    .Permission(Permissions.ManageSettings)));
        }
    }
}