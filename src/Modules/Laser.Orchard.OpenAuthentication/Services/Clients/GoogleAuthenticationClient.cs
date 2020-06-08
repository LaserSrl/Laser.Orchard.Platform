using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.Messaging;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard.Logging;
using Laser.Orchard.OpenAuthentication.Security;
using System.Text.RegularExpressions;
using Laser.Orchard.OpenAuthentication.Extensions;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Web;
using Orchard.Localization;
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class GoogleAuthenticationClient : IExternalAuthenticationClient {
        public GoogleAuthenticationClient() {
            Logger = NullLogger.Instance;
        }

        public string ProviderName {
            get { return "Google"; }
        }

        public ILogger Logger { get; set; }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            string ClientId = providerConfiguration.ProviderIdKey;
            string ClientSecret = providerConfiguration.ProviderSecret;
            var client = new GoogleOAuth2Client(ClientId, ClientSecret);
            return client;
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal = createUserParams;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "mail") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previosAuthResult, string userAccessToken) {
            var userData = (Build(providerConfiguration) as GoogleOAuth2Client).GetUserDataDictionary(userAccessToken);
            //Logger.Error("user data count: {0}", userData.Count);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string name = userData["email"];
            userData["name"] = userData["email"];
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            var client = Build(providerConfiguration) as GoogleOAuth2Client;
            //Logger.Error("Inizio chiamata Google");
            string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            //Logger.Error("access token: {0}", userAccessToken);
            return GetUserData(providerConfiguration, previousAuthResult, userAccessToken);
        }
        
        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return new Dictionary<string, LocalizedString>();
        }
    }
}