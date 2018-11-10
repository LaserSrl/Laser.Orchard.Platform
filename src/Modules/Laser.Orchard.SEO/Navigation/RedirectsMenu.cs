using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.SEO.Navigation {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectsMenu : INavigationProvider {

        public RedirectsMenu() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public string MenuName {
            get {
                return "admin";
            }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("SEO"), "10", menu => menu.LinkToFirstChild(false).Permission(RedirectPermissions.ConfigureRedirects)
                    .Add(item => item
                        .Caption(T("Redirects"))
                        .Position("0")
                        .Action("Index", "RedirectsAdmin", new { area = "Laser.Orchard.SEO" })
                        .Permission(RedirectPermissions.ConfigureRedirects)
                    ));
        }
    }
}