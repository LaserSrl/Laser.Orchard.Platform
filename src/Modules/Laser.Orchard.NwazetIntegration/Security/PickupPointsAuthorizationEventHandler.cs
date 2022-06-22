using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core = Orchard.Core;

namespace Laser.Orchard.NwazetIntegration.Security {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsAuthorizationEventHandler
        : IAuthorizationServiceEventHandler {
        public void Adjust(CheckAccessContext context) {
            // only users with the MayConfigurePickupPoints Permission are allowed
            // to do anything on backend to ContentItems with a PickupPointPart
            if (context.Content.Is<PickupPointPart>()
                && IsCoreManagePermission(context.Permission)) {
                context.Granted = false;
                context.Adjusted = true;
                context.Permission = PickupPointPermissions.MayConfigurePickupPoints;
            }
        }

        // Is the permission any of the "backoffice" permissions from Orchard.Core.Contents?
        private bool IsCoreManagePermission(Permission perm) {
            return perm == Core.Contents.Permissions.CreateContent
                || perm == Core.Contents.Permissions.PublishContent
                || perm == Core.Contents.Permissions.PublishOwnContent
                || perm == Core.Contents.Permissions.EditContent
                || perm == Core.Contents.Permissions.EditOwnContent
                || perm == Core.Contents.Permissions.DeleteContent
                || perm == Core.Contents.Permissions.DeleteOwnContent;
        }

        #region Not Implemented
        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) { }
        #endregion
    }
}