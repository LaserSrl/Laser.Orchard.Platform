using Laser.Orchard.SecureData.Fields;
using Orchard.ContentManagement;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Security {
    public class EncryptedStringFieldEditPermission : Permission {
        public ContentPart Part { get; set; }
        public EncryptedStringField Field { get; set; }

        public EncryptedStringFieldEditPermission(ContentPart part, EncryptedStringField field) {
            Name = "EncryptedStringFieldEditPermission_" + part.PartDefinition.Name + "." + field.Name;
            Part = part;
            Field = field;
            ImpliedBy = new Permission[] {
               EncryptedStringFieldPermissions.ManageAllEncryptedStringFields
            };
        }
    }
}