using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Orchard;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public interface IExternalAuthenticationClient : IDependency {
        string ProviderName { get; }
        
        IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord);

        AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken);
        AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl);
        
        OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData);

        /// <summary>
        /// Return true if this method rewrote request, false otherwise.
        /// </summary>
        /// <returns></returns>
        bool RewriteRequest();
    }
}