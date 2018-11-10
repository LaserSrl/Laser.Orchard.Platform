using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Laser.Orchard.CulturePicker.Navigation {
    public class CulturePickerNavigationProvider : INavigationProvider {
        public CulturePickerNavigationProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),
                menu => menu.Add(T("Culture Picker"), "1", item => item.Action("Settings", "Admin", new { area = "Laser.Orchard.CulturePicker" })
                    .Permission(StandardPermissions.SiteOwner)));
        }
    }
}