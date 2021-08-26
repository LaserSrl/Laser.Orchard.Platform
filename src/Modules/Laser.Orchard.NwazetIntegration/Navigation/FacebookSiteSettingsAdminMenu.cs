using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.NwazetIntegration.Navigation {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookSiteSettingsAdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public FacebookSiteSettingsAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu
                .Add(T("Facebook Shop"), "2.1"));
        }
    }
}