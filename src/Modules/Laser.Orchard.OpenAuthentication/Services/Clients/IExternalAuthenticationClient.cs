using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard;
using Orchard.Localization;
using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public interface IExternalAuthenticationClient : IDependency {
        string ProviderName { get; }
        
        IAuthenticationClient Build(ProviderConfigurationViewModel providerConfigurationRecord);

        AuthenticationResult GetUserData(ProviderConfigurationViewModel clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken);
        AuthenticationResult GetUserData(ProviderConfigurationViewModel clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl);
        
        OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData);

        /// <summary>
        /// Return true if this method rewrote request, false otherwise.
        /// </summary>
        /// <returns></returns>
        bool RewriteRequest();
        /// <summary>
        /// Returns keys and labels of the additional attributes for this provider.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, LocalizedString> GetAttributeKeys();
    }
}