using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.Users.Models;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiCredentialsManagementService : IDependency {
        /// <summary>
        /// Returns the plain text secret for the user represented by the part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        string GetSecret(ApiCredentialsPart part);
        /// <summary>
        /// Generate a new set of credentials for the user represented by the part.
        /// </summary>
        /// <param name="part"></param>
        void GenerateNewCredentials(ApiCredentialsPart part);
        /// <summary>
        /// Validates the given credentials and returns the corresponding orchard User (if any)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        UserPart ValidateCredentials(string key, string secret);
    }
}
