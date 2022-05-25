using Laser.Orchard.HiddenFields.Fields;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Services {
    public interface IHiddenFieldService : IDependency {
        Permission GetOwnPermission(ContentPart part, HiddenStringField field);
        Permission GetAllPermission(ContentPart part, HiddenStringField field);
        Permission GetOwnHiddenPermission(string partName, string fieldName);
        Permission GetAllHiddenPermission(string partName, string fieldName);

        Permission GetSeeAllHiddenPermission(string partName, string fieldName);
        Permission GetSeeOwnPermission(ContentPart part, HiddenStringField field);
        Permission GetSeeOwnHiddenPermission(string partName, string fieldName);
        Permission GetSeeAllPermission(ContentPart part, HiddenStringField field);

    }

    public enum HiddenStringFieldUpdateProcessVariant {
        None,
        All,
        Empty
    }
}