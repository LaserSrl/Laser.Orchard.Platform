
using Framework = Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.AppleEnvironment.Permissions {
    [OrchardFeature("Laser.Orchard.ApplePay.DomainAssociation")]
    public class MerchantDomainAssociationPermissions : IPermissionProvider {

        public virtual Framework.Feature Feature { get; set; }

        public static readonly Permission MerchantDomainAssociationFile =
            new Permission { Description = "Merchant Domain Association File", Name = "Merchant Domain Association File for ApplePay" };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] { new PermissionStereotype { Name = "Administrator", Permissions = new[] { MerchantDomainAssociationFile } } };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { MerchantDomainAssociationFile };
        }
    }
}