using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class YahooAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Yahoo"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            return new YahooOpenIdClient();
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            string id = previousAuthResult.ProviderUserId;
            string name = previousAuthResult.UserName;
            return new AuthenticationResult(true, ProviderName, id, name, previousAuthResult.ExtraData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            return GetUserData(clientConfiguration, previousAuthResult, token);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            return createUserParams;
        }

        public bool RewriteRequest() {
            return false;
        }
    }
}