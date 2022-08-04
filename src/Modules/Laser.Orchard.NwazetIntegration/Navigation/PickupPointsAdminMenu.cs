using Laser.Orchard.NwazetIntegration.Security;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Laser.Orchard.NwazetIntegration.Navigation {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsAdminMenu : INavigationProvider {
        public string MenuName {
            get { return "admin"; }
        }

        public PickupPointsAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            // TODO: The "page" to manage PickupPoints will live unders the same
            // menu as the Shipping configurations.
            builder
                .AddImageSet("nwazet-commerce")
                .Add(item => item
                    .Caption(T("Commerce"))
                    .Position("2")
                    .LinkToFirstChild(false)

                    .Add(subItem => subItem
                        .Position("2.7") 
                        .Caption(T("Pickup Points"))
                        .Permission(PickupPointPermissions.MayConfigurePickupPoints)
                        .Action("List", "PickupPointsAdmin", new { area = "Laser.Orchard.NwazetIntegration" })
                    )
                );
        }
    }
}