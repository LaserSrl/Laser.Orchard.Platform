using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Models {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationRecord {
        public virtual int Id { get; set; }
        public virtual string FileContent { get; set; }
        public virtual bool Enable { get; set; }
    }
}