using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsPart : ContentPart<UsersGroupsPartRecord> {
        internal LazyField<IList<ExtendedUsersGroupsRecord>> _userGroups = new LazyField<IList<ExtendedUsersGroupsRecord>>();
        public string UserGroup {
            get { return this.Retrieve(r => r.theUserGroup); }
            set { this.Store(r => r.theUserGroup, value); }
        }

        public IList<ExtendedUsersGroupsRecord> UserGroups {
            get {
                return _userGroups.Value;
            }
        }

    }
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsPartRecord : ContentPartRecord {
        public virtual string theUserGroup { get; set; }

    }
}
