using Laser.Orchard.GDPR.Permissions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.GDPR.Navigation {
    public class GDPRAdminMenu : INavigationProvider {


        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(item => {
                item = item
                    .Caption(T("Profile items"))
                    .Position("3")
                    .Action("Index", "GDPRAdmin", new { area = "Laser.Orchard.GDPR" })
                    .Permission(GDPRPermissions.ManageAnonymization)
                    .Permission(GDPRPermissions.ManageErasure);
                // when we have dynamic permissions on each type, we can add them here in
                // a loop
            });
        }
    }
}