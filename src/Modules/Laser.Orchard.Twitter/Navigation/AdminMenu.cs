using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.Twitter.Navigation {

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
                .Caption(T("Social Twitter Account"))
                .Position("1.064")
                .Action("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter" })
                .Permission(Permissions.ManageTwitterAccount)
       );
        }
    }
}