using System;
using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web.Hosting;
using Orchard.Environment.Configuration;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using Orchard.Logging;
using Orchard.Localization;
using Orchard.Data;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class AppleAuthenticationClient : IExternalAuthenticationClient {
        private readonly ShellSettings _shellSetting;
        private readonly IRepository<ProviderConfigurationRecord> _repository;
        private readonly IRepository<ProviderAttributeRecord> _repositoryAttributes;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public string ProviderName => "Apple";

        public AppleAuthenticationClient(
            ShellSettings shellSetting, 
            IRepository<ProviderConfigurationRecord> repository,
            IRepository<ProviderAttributeRecord> repositoryAttributes) {
            _shellSetting = shellSetting;
            _repository = repository;
            _repositoryAttributes = repositoryAttributes;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            var providerAttributes = GetAttributes(providerConfigurationRecord.Id);
            string clientId = providerAttributes["ServicesID"];
            string clientSecret = GetClientSecret(providerAttributes, false);
            var client = new AppleOAuth2Client(clientId, clientSecret, Logger);
            return client;
        }

        public IAuthenticationClient BuildMobile(ProviderConfigurationRecord providerConfigurationRecord) {
            var providerAttributes = GetAttributes(providerConfigurationRecord.Id);
            string clientId = providerAttributes["BundleID"];
            string clientSecret = GetClientSecret(providerAttributes, true);
            var client = new AppleOAuth2Client(clientId, clientSecret, Logger);
            return client;
        }

        private string GetClientSecret(Dictionary<string, string> providerAttributes, bool forMobile) {
            var epoch = new DateTime(1970, 1, 1);
            var payload = new Dictionary<string, object>() {
                { "iss", providerAttributes["TeamID"] },
                { "iat", (DateTime.UtcNow.AddMinutes(-1) - epoch).TotalSeconds },
                { "exp", (DateTime.UtcNow.AddMonths(5) - epoch).TotalSeconds },
                { "aud", "https://appleid.apple.com" },
                { "sub", forMobile? providerAttributes["BundleID"] : providerAttributes["ServicesID"] },
            };
            var extraHeader = new Dictionary<string, object>() {
                { "alg", "ES256" },
                { "typ", "JWT" },
                { "kid", providerAttributes["KeyID"] }
            };
            var p8File = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\" + providerAttributes["p8File"];
            string content = File.ReadAllText(p8File);
            string[] keyLines = content.Split('\n');
            content = string.Join(string.Empty, keyLines.Skip(1).Take(keyLines.Length - 2));
            byte[] keyBlob = Convert.FromBase64String(content);
            using (var privateKey = CngKey.Import(keyBlob, CngKeyBlobFormat.Pkcs8PrivateBlob)) {
                var headerString = JsonConvert.SerializeObject(extraHeader);
                var payloadString = JsonConvert.SerializeObject(payload);
                using (ECDsaCng dsa = new ECDsaCng(privateKey)) {
                    dsa.HashAlgorithm = CngAlgorithm.Sha256;
                    var unsignedJwtData = Base64UrlEncode(headerString) + "." + Base64UrlEncode(payloadString);
                    var signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
                    return unsignedJwtData + "." + Base64UrlEncode(signature);
                }
            }
        }

        private static string Base64UrlEncode(string input) {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Base64UrlEncode(inputBytes);
        }
        private static string Base64UrlEncode(byte[] inputBytes) {
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            if (previousAuthResult != null && previousAuthResult.IsSuccessful && !string.IsNullOrWhiteSpace(previousAuthResult.Provider)) {
                return previousAuthResult;
            }
            else {
                var userData = (Build(clientConfiguration) as AppleOAuth2Client).GetUserDataDictionary(userAccessToken);
                userData["accesstoken"] = userAccessToken;
                var id = userData["sub"];
                var email = userData["email"];
                return new AuthenticationResult(true, this.ProviderName, id, email, userData);
            }
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl) {
            if (previousAuthResult != null && previousAuthResult.IsSuccessful && ! string.IsNullOrWhiteSpace(previousAuthResult.Provider)) {
                return previousAuthResult;
            }
            else {
                var client = BuildMobile(clientConfiguration) as AppleOAuth2Client;
                string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
                return GetUserData(clientConfiguration, previousAuthResult, userAccessToken);
            }
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            return clientData;
        }

        public bool RewriteRequest() {
            return RewriteRequestByPostState();
        }
        private bool RewriteRequestByPostState() {
            bool result = false;
            var ctx = System.Web.HttpContext.Current;
            var stateString = System.Web.HttpUtility.HtmlDecode(ctx.Request.Form["state"]);
            if (stateString != null && stateString.Contains("__provider__=Apple")) {
                // this provider requires that all return data be packed into a "state" parameter
                var q = System.Web.HttpUtility.ParseQueryString(stateString);
                var codeString = System.Web.HttpUtility.HtmlDecode(ctx.Request.Form["code"]);
                if( ! string.IsNullOrWhiteSpace(codeString)) {
                    var q2 = System.Web.HttpUtility.ParseQueryString(codeString);
                    q.Add("code", codeString);
                }
                q.Add(ctx.Request.QueryString);
                ctx.RewritePath(ctx.Request.Path + "?" + q.ToString());
                result = true;
            }
            return result;
        }
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return new Dictionary<string, LocalizedString>() {
                { "KeyID", T("Key ID") },
                { "TeamID", T("Team ID") },
                { "ServicesID", T("Services ID") },
                { "BundleID", T("Bundle ID") },
                { "p8File", T("Certificate file name (for instance: AuthKey_XXXXXXXXXX.p8) to be placed in App_Data/Sites/tenant folder") }
            };
        }
        private Dictionary<string, string> GetAttributes(int providerId) {
            var result = new Dictionary<string, string>();
            var provider = _repository.Get(providerId);
            if (provider != null) {
                var original = _repositoryAttributes.Fetch(x => x.ProviderId == providerId).ToList();
                foreach (var item in GetAttributeKeys()) {
                    var origValue = original.FirstOrDefault(x => x.AttributeKey == item.Key);
                    result.Add(item.Key, origValue != null ? origValue.AttributeValue : "");
                }
            }
            return result;
        }
    }
}