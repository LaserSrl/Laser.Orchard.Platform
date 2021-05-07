using Laser.Orchard.SecureData.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Security {
    public class HashedStringFieldAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly ISecureFieldService _secureFieldService;

        public HashedStringFieldAuthorizationEventHandler(ISecureFieldService secureFieldService) {
            _secureFieldService = secureFieldService;
        }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted && context.Permission is HashedStringFieldEditPermission) {
                var fieldPermission = context.Permission as HashedStringFieldEditPermission;
                if (HasOwnership(context.User, context.Content)) {
                    // Own Permission.
                    context.Permission = _secureFieldService.GetOwnPermission(fieldPermission.Part, fieldPermission.Field);
                    context.Adjusted = true;
                } else {
                    // All Permission.
                    context.Permission = _secureFieldService.GetAllPermission(fieldPermission.Part, fieldPermission.Field); ;
                    context.Adjusted = true;
                }
            }
        }

        public void Checking(CheckAccessContext context) {

        }

        public void Complete(CheckAccessContext context) {

        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }
    }
}