using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class InstagramAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Instagram"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            string ClientId = providerConfiguration.ProviderIdKey;
            string ClientSecret = providerConfiguration.ProviderSecret;
            var client = new InstagramOAuth2Client(ClientId, ClientSecret);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            var userData = (Build(providerConfiguration) as InstagramOAuth2Client).GetUserDataDictionary(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string username = userData["username"];
            return new AuthenticationResult(true, this.ProviderName, id, username, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl) {
            var client = Build(providerConfiguration) as InstagramOAuth2Client;
            string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            return GetUserData(providerConfiguration, previousAuthResult, userAccessToken);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            return clientData;
        }

        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return new Dictionary<string, LocalizedString>();
        }
    }
}