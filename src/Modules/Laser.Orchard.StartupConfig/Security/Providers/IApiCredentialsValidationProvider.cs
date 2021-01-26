using Laser.Orchard.StartupConfig.Models;
using Orchard;

namespace Laser.Orchard.StartupConfig.Security.Providers {
    public interface IApiCredentialsValidationProvider : IDependency {
        /// <summary>
        /// Tests whether the user represented by the part is allowed to sign in
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        bool ValidateSignIn(ApiCredentialsPart part);
    }
}
