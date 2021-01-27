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
            // TODO: in case we ever decide to have a specific permission, we should take care
            // to where the menu link ends up, and what it shows. For example, if the "Settings"
            // menu is shown, which of its children are also shown by default?
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