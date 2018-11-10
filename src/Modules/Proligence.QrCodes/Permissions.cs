using Orchard.Security.Permissions;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.ContentManagement;


namespace Proligence.QrCodes {
    public class Permissions : IPermissionProvider {
        public static readonly Permission EditQrCode = new Permission { Description = "Edit QRCode", Name = "EditQrCode" };
        private readonly IContentManager _contentManager;
        public Localizer T;
        public Feature Feature { get; set; }
        public Permissions(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { EditQrCode}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                 EditQrCode
            };
        }
      
    }
}