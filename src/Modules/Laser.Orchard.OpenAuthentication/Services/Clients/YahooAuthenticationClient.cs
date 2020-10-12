using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Newtonsoft.Json;
using Orchard.Localization;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class YahooAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Yahoo"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            return new YahooOpenIdClient();
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            string id = previousAuthResult.ProviderUserId;
            string name = previousAuthResult.UserName;
            return new AuthenticationResult(true, ProviderName, id, name, previousAuthResult.ExtraData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            return GetUserData(providerConfiguration, previousAuthResult, token);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            return createUserParams;
        }

        public bool RewriteRequest() {
            return false;
        }
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return new Dictionary<string, LocalizedString>();
        }
    }
}