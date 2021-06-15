using Laser.Orchard.HiddenFields.Fields;
using Orchard.ContentManagement;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Security {
    public class HiddenFieldEditPermission : Permission {
        public ContentPart Part { get; set; }
        public HiddenStringField Field { get; set; }

        public HiddenFieldEditPermission(ContentPart part, HiddenStringField field) {
            Name = "HiddenFieldEditPermission_" + part.PartDefinition.Name + "." + field.Name;
            Part = part;
            Field = field;
            ImpliedBy = new Permission[] {
                HiddenFieldsPermissions.MayEditHiddenFields
            };
        }
    }
}