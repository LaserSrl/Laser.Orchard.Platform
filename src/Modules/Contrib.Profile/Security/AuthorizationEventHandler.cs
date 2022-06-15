using Orchard.Security;
using Orchard.Core.Contents;
using Orchard;
using System;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Contrib.Profile.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IWorkContextAccessor _workContextAccessor;

        public AuthorizationEventHandler(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            var permission = Permissions.ViewOwnProfile;
            if (context.Granted || context.User == null) {
                return;
            }
            if (context.Permission == Permissions.ViewProfiles) {
                if (context.Content.As<UserPart>() != context.User) {
                    return;
                }
            }
            // Testing the ViewContent Permission for a SecureFileField hosted in a User 
            else if (context.Permission == Orchard.Core.Contents.Permissions.ViewContent) {
                var routeData = _workContextAccessor.GetContext().HttpContext.Request.RequestContext.RouteData.Values;

                //If the invoking controller/action is NOT SecureFileField/GetSecureFile, returns;
                if (!string.Format("{0}/{1}/{2}", routeData["area"], routeData["controller"], routeData["action"]).ToString().Equals("CloudConstruct.SecureFileField/SecureFileField/GetSecureFile", StringComparison.InvariantCultureIgnoreCase)) {
                    return;
                }
                //If the content is not a User;
                else if (context.Content.As<UserPart>() == null) {
                    return;
                }
                else {
                    //If content is an user and does not match with the current User, we check the ViewProfiles instead of the ViewContent
                    if (context.Content.As<UserPart>() != context.User) {
                        permission = Permissions.ViewProfiles;
                    }
                    //Otherwise (content match with the current user) we check the ViewOwnProfile instead of ViewOwnContent
                    else {
                        permission = Permissions.ViewOwnProfile;
                    }
                }
            }
            context.Adjusted = true;
            context.Permission = permission;
        }
    }
}