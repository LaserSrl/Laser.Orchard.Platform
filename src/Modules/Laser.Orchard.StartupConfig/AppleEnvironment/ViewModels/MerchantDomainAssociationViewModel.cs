using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.ViewModels {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationViewModel {
        public string Text { get; set; }
        public bool Enable { get; set; }
    }
}