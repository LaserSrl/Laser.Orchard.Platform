using Laser.Orchard.HiddenFields.Fields;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Services {
    public class HiddenFieldService : IHiddenFieldService {

        public Localizer T { get; set; }

        public HiddenFieldService() {
            T = NullLocalizer.Instance;
        }

        public Permission GetAllHiddenPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("Edit all {0} hidden string fields", fieldFullName).Text,
                Name = "MayEditHiddenFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HiddenFieldsPermissions.MayEditHiddenFields
                }
            };
        }

        public Permission GetAllPermission(ContentPart part, HiddenStringField field) {
            return GetAllHiddenPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetOwnHiddenPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("Edit own {0} hidden string fields", fieldFullName).Text,
                Name = "MayEditOwnHiddenFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HiddenFieldsPermissions.MayEditOwnHiddenFields,
                    GetAllHiddenPermission(partName, fieldName)
                }
            };
        }
        public Permission GetOwnPermission(ContentPart part, HiddenStringField field) {
            return GetOwnHiddenPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetSeeAllHiddenPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("See all {0} hidden string fields", fieldFullName).Text,
                Name = "MaySeeAllHiddenFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HiddenFieldsPermissions.MaySeeHiddenFields
                }
            };
        }

        public Permission GetSeeOwnHiddenPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("See own {0} hidden string fields", fieldFullName).Text,
                Name = "MaySeeOwnHiddenFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HiddenFieldsPermissions.MaySeeHiddenFields
                }
            };
        }
    }
}