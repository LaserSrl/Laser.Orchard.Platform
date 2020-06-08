using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class MicrosoftAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Microsoft"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            return new MicrosoftClient(providerConfiguration.ProviderIdKey, providerConfiguration.ProviderSecret, "wl.basic", "wl.emails");
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            Dictionary<string, string> userData = new Dictionary<string, string>();
            string uri = "https://apis.live.net/v5.0/me?access_token=" + userAccessToken;
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse()) {
                using (var stream = webResponse.GetResponseStream()) {
                    if (stream == null)
                        return null;

                    using (var textReader = new StreamReader(stream)) {
                        var json = textReader.ReadToEnd();
                        var valori = JObject.Parse(json);
                        var data = valori.Root; // .SelectToken("data");
                        userData.Add("id", data.Value<string>("id"));
                        userData.Add("name", data.Value<string>("name"));
                        userData.Add("first_name", data.Value<string>("first_name"));
                        userData.Add("last_name", data.Value<string>("last_name"));
                        userData.Add("gender", data.Value<string>("gender"));
                        userData.Add("locale", data.Value<string>("locale"));
                        userData.Add("updated_time", data.Value<string>("updated_time"));
                        var emails = valori.SelectToken("emails");
                        userData.Add("email", emails.Value<string>("preferred"));
                    }
                }
            }

            string id = userData["id"];
            string name = userData["name"];

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = userAccessToken;
            return new AuthenticationResult(true, ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previosAuthResult, string token, string userAccessSecret, string returnUrl) {
            return GetUserData(providerConfiguration, previosAuthResult, token);
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