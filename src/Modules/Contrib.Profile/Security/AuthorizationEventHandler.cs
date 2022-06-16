using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using System;

namespace Contrib.Profile.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IWorkContextAccessor _workContextAccessor;

        public AuthorizationEventHandler(
            IWorkContextAccessor workContextAccessor) {

            _workContextAccessor = workContextAccessor;
        }

        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            // If the authorization is already granted, nothing to adjust here
            if (context.Granted) {
                return;
            }
            // If we are testing the ViewProfiles Permission, we may have to adjust for the Own permission
            // as long as the content IS the user
            if (context.Permission == Permissions.ViewProfiles && context.Content == context.User) {
                context.Adjusted = true;
                context.Permission = Permissions.ViewOwnProfile;
                return;
            }
            // If we are testing the ViewContent Permission, check whether the test comes from the path of
            // the SecureFileField controller, for a field that is hosted in a User
            if (context.Permission == Orchard.Core.Contents.Permissions.ViewContent) {
                var routeData = _workContextAccessor.GetContext()
                    .HttpContext.Request.RequestContext.RouteData.Values;
                if ("CloudConstruct".Equals(routeData["area"].ToString(), StringComparison.OrdinalIgnoreCase)
                    && "SecureFileField".Equals(routeData["controller"].ToString(), StringComparison.OrdinalIgnoreCase)
                    && "GetSecureFile".Equals(routeData["action"].ToString(), StringComparison.OrdinalIgnoreCase)
                    ) {
                    // The check for authorizations has been invoked in a call to the SecureFileField controller
                    var userPart = context.Content.As<IUser>();
                    if (userPart == null) {
                        // We only adjust permissions when the content is a user
                        return;
                    }
                    // If the user matches the current user, adjust to the Own permission, otherwise adjust to the
                    // generic one
                    if (userPart == context.User) {
                        context.Permission = Permissions.ViewOwnProfile;
                    } else {
                        context.Permission = Permissions.ViewProfiles;
                    }
                    context.Adjusted = true;
                    return;
                }
            }
            // No other case to handle

        }
    }
}