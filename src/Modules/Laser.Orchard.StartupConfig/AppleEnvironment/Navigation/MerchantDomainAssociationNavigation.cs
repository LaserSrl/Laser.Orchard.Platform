using Orchard.Localization;
using Orchard.UI.Navigation;
using Laser.Orchard.StartupConfig.AppleEnvironment.Permissions;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Navigation {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationNavigation : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public MerchantDomainAssociationNavigation() {
            T = NullLocalizer.Instance;
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"), menu => menu.LinkToFirstChild(false).Permission(MerchantDomainAssociationPermissions.MerchantDomainAssociationFile)
                   .Add(item => item
                       .Caption(T("ApplePay Merchant Domain Association File"))
                       .Position("10")
                       .Action("Index", "MerchantDomainAssociationAdmin", new { area = "Laser.Orchard.StartupConfig" })
                       .Permission(MerchantDomainAssociationPermissions.MerchantDomainAssociationFile)
                   ));
        }
    }
}