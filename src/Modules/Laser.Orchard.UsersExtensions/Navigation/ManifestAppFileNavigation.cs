using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.UsersExtensions.Navigation {
    public class ManifestAppFileNavigation : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public ManifestAppFileNavigation() {
            T = NullLocalizer.Instance;
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),  menu => menu.LinkToFirstChild(false)//.Permission(Permissions.ConfigureRobotsTextFile) TODO  permission
                    .Add(item => item
                        .Caption(T("Manifest App File"))
                        .Position("10")
                        .Action("Index", "ManifestAppFileAdmin", new { area = "Laser.Orchard.UsersExtensions" })
                        //.Permission(Permissions.ConfigureRobotsTextFile)
                    ));
        }
    }
}