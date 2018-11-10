using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Laser.Orchard.ContentExtension.Navigation {
    public class ContentExtensionNavigationProvider : INavigationProvider {
        public ContentExtensionNavigationProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),
                menu => menu.Add(T("ContentType Permission"), "1", item => item.Action("Settings", "Admin", new { area = "Laser.Orchard.ContentExtension" })
                    .Permission(StandardPermissions.SiteOwner)));
        }
    }
}