using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.AdvancedSearch {
    [OrchardFeature("Laser.Orchard.AdvancedSearch")]
    public class ContentAdminMenu : INavigationProvider {
        public ContentAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Content"),
                menu => menu
                    .Add(T("Advanced Search"), "0.5", item => item.Action("List", "Admin", new { area = "Laser.Orchard.AdvancedSearch" }).LocalNav())
                );

        }
    }
}