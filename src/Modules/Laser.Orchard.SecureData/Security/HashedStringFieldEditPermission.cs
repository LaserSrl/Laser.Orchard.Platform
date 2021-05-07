using Laser.Orchard.SecureData.Fields;
using Orchard.ContentManagement;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Security {
    public class HashedStringFieldEditPermission : Permission {
        public ContentPart Part { get; set; }
        public HashedStringField Field { get; set; }

        public HashedStringFieldEditPermission(ContentPart part, HashedStringField field) {
            Name = "HashedStringFieldEditPermission_" + part.PartDefinition.Name + "." + field.Name;
            Part = part;
            Field = field;
            ImpliedBy = new Permission[] {
               HashedStringFieldPermissions.ManageAllHashedStringFields
            };
        }
    }
}
