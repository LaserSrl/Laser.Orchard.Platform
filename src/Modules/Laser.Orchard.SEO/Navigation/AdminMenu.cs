using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.SEO.Navigation {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("SEO"), "10", menu => menu.LinkToFirstChild(false).Permission(Permissions.ConfigureRobotsTextFile)
                    .Add(item => item
                        .Caption(T("Robots.txt"))
                        .Position("0")
                        .Action("Index", "RobotsAdmin", new { area = "Laser.Orchard.SEO" })
                        .Permission(Permissions.ConfigureRobotsTextFile)
                    ));
        }
    }
}