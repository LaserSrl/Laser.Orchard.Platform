using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Laser.Orchard.WebServices.Navigation {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class CustomRestApiSettingsAdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public CustomRestApiSettingsAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(item => item
                    .Caption(T("Settings"))
                    .Add(subItem => subItem
                        .Caption(T("Custom REST API"))
                        .Position("2.1")
                        .Action("Index", "CustomRestApiSettingsAdmin", new { area = "Laser.Orchard.WebServices" })
                        .Permission(StandardPermissions.SiteOwner)
                    )
                );
        }
    }
}