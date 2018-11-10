using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Queries.Navigation {

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
                .Caption(T("Custom Query"))
                .Permission(Permissions.UserQuery)
                .Position("1.07")
                .Action("Index", "MyQueryAdmin", new { area = "Laser.Orchard.Queries" })
                .AddImageSet("CustomQuery")
                
       );
        }
    }
}