using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Laser.Orchard.StartupConfig.Security.Providers {
    public class UserApprovedApiCredentialsValidationProvider
        : IApiCredentialsValidationProvider {
        public bool ValidateSignIn(ApiCredentialsPart part) {
            if (part == null) {
                return false;
            }
            var user = part.As<UserPart>();
            if (user == null) {
                return false;
            }

            if (user.EmailStatus != UserStatus.Approved)
                return false;

            if (user.RegistrationStatus != UserStatus.Approved)
                return false;

            return true;
        }
    }
}