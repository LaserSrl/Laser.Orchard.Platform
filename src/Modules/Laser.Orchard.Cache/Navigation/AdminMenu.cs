using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Cache.Navigation {

    public class AdminMenu : INavigationProvider {

        public string MenuName {
            get { return "admin"; }
        }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => item
                .Caption(T("Cache URL"))
                .Permission(Permissions.UrlCache)
                .Position("10")
                .Action("Index", "CacheUrlAdmin", new { area = "Laser.Orchard.Cache" })
                .AddImageSet("CacheUrl")
                );
        }
    }
}