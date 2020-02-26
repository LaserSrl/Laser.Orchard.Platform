using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using System.Web;
using System;
using Orchard.Localization;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class LinkedInAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "LinkedIn"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string ClientId = providerConfigurationRecord.ProviderIdKey;
            string ClientSecret = providerConfigurationRecord.ProviderSecret;
            var client = new LinkedInOAuth2Client(ClientId, ClientSecret);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            var userData = (Build(clientConfiguration) as LinkedInOAuth2Client).GetUserDataDictionary(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string name = userData["email-address"];
            userData["name"] = userData["email-address"];
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            var client = Build(clientConfiguration) as LinkedInOAuth2Client;
            string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            return GetUserData(clientConfiguration, previousAuthResult, userAccessToken);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams createUserParams) {
            OpenAuthCreateUserParams retVal = createUserParams;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {
                if (values.Key == "email-address") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }

        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return new Dictionary<string, LocalizedString>();
        }
    }
}