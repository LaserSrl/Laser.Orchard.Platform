using Orchard.Security;

namespace Contrib.Profile.Security {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler
    {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (context.Granted || context.Permission != Permissions.ViewProfiles || context.Content != context.User) {
                return;
            }

            context.Adjusted = true;
            context.Permission = Permissions.ViewOwnProfile;
        }
    }
}