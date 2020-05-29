using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Orchard.Logging;
using System.Linq;
using Orchard.Localization;
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class TwitterAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Twitter"; }
        }

        public ILogger Logger { get; set; }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            return new TwitterCustomClient(providerConfiguration.ProviderIdKey, providerConfiguration.ProviderSecret);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            string secret = "";
            if (previousAuthResult.ExtraData.ContainsKey("secret")) {
                secret = previousAuthResult.ExtraData["secret"];
            }
            return GetUserData(providerConfiguration, previousAuthResult, userAccessToken, secret, "");
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecret, string returnUrl) {
            var userAccessToken = token;
            if (String.IsNullOrWhiteSpace(userAccessSecret)) {
                if (previousAuthResult.ExtraData.ContainsKey("accesstoken") == false) {
                    previousAuthResult.ExtraData.Add("accesstoken", userAccessToken);
                }
                return new AuthenticationResult(true, this.ProviderName, previousAuthResult.ProviderUserId, previousAuthResult.UserName, previousAuthResult.ExtraData);
            }
            var twitterUserSerializer = new DataContractJsonSerializer(typeof(TwitterUserData));

            TwitterUserData twitterUserData;
            // recupero lo User_Name e la mail relativi al token
            var accountSettingsRequest = PrepareAuthorizedRequestGet(userAccessToken,
                userAccessSecret,
                providerConfiguration.ProviderIdKey,
                providerConfiguration.ProviderSecret,
                "https://api.twitter.com/1.1/account/verify_credentials.json?skip_status=true&include_email=true");
            try {
                using (var response = accountSettingsRequest.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {
                        twitterUserData = (TwitterUserData)twitterUserSerializer.ReadObject(responseStream);
                        if (String.IsNullOrWhiteSpace(twitterUserData.Screen_Name)) {
                            Logger.Error("Twitter: missing screen_name");
                            return AuthenticationResult.Failed;
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Twitter verify_credentials");
                return AuthenticationResult.Failed;
            }

            var userData = new Dictionary<string, string>();
            userData["id"] = userAccessToken.Split('-')[0];
            userData["username"] = twitterUserData.Screen_Name;
            userData["email"] = twitterUserData.Email;

            string id = userData["id"];
            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
                name = id;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = userAccessToken;

            return new AuthenticationResult(
                isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
        }

        private HttpWebRequest PrepareAuthorizedRequestGet(string oauth_token, string oauth_token_secret, string oauth_consumer_key, string oauth_consumer_secret, string resource_url) {
            // NB: per gestire il POST bisogna aggiungere alla variabile dic anche i dati del body
            string httpMethod = "GET";
            
            // normalizza l'url togliendo eventuali parametri da query string
            Uri urlToCall = new Uri(resource_url);
            var normalizedResourceUrl = String.Format("{0}{1}{2}{3}", urlToCall.Scheme, Uri.SchemeDelimiter, urlToCall.Authority, urlToCall.AbsolutePath);

            // estrae eventuali parametri da query string
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string queryString = urlToCall.Query;
            if (queryString.StartsWith("?")) {
                queryString = queryString.Substring(1);
            }
            string[] queryParameters = queryString.Split('&');
            string[] arr = null;
            foreach (var kv in queryParameters) {
                arr = kv.Split('=');
                dic.Add(arr[0], arr[1]);
            }

            // aggiunge i parametri oauth
            dic.Add("oauth_consumer_key", oauth_consumer_key);
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            dic.Add("oauth_nonce", oauth_nonce);
            var oauth_signature_method = "HMAC-SHA1";
            dic.Add("oauth_signature_method", oauth_signature_method);
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            dic.Add("oauth_timestamp", oauth_timestamp);
            dic.Add("oauth_token", oauth_token);
            var oauth_version = "1.0";
            dic.Add("oauth_version", oauth_version);

            // ordina tutti i parametri in ordine alfabetico per chiave
            var orderedDic = dic.OrderBy(x => x.Key).ToDictionary(k => k.Key, v => v.Value);

            StringBuilder sb = new StringBuilder();
            foreach (var par in orderedDic) {
                sb.AppendFormat("{0}={1}&", par.Key, par.Value);
            }
            //elimina l'ultimo &
            var baseString = sb.ToString();
            baseString = baseString.TrimEnd('&');

            // completa la base string
            baseString = string.Concat(httpMethod.ToUpper(), "&", Uri.EscapeDataString(normalizedResourceUrl),
                "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                    "&", Uri.EscapeDataString(oauth_token_secret));

            // compone l'header
            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey))) {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(oauth_consumer_key),
                                    Uri.EscapeDataString(oauth_token),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );

            ServicePointManager.Expect100Continue = false;

            // crea la request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = httpMethod.ToUpper();
            return request;
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