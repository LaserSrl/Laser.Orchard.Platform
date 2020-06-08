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
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class AppleAuthenticationClient : IExternalAuthenticationClient {
        private readonly ShellSettings _shellSetting;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public string ProviderName => "Apple";

        public AppleAuthenticationClient(
            ShellSettings shellSetting) {
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            var attrbutesDictionary = new Dictionary<string, string>();
            var providerAttributes = providerConfiguration.Attributes.ToDictionary(k => k.AttributeKey, v => v.AttributeValue);
            string clientId = providerAttributes["ServicesID"];
            string clientSecret = GetClientSecret(providerAttributes, false);
            var client = new AppleOAuth2Client(clientId, clientSecret, Logger);
            return client;
        }

        public IAuthenticationClient BuildMobile(ProviderConfigurationViewModel providerConfiguration) {
            var attrbutesDictionary = new Dictionary<string, string>();
            var providerAttributes = providerConfiguration.Attributes.ToDictionary(k => k.AttributeKey, v => v.AttributeValue);
            string clientId = providerAttributes["BundleID"];
            string clientSecret = GetClientSecret(providerAttributes, true);
            var client = new AppleOAuth2Client(clientId, clientSecret, Logger);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            if (previousAuthResult != null && previousAuthResult.IsSuccessful && !string.IsNullOrWhiteSpace(previousAuthResult.Provider)) {
                return previousAuthResult;
            }
            else {
                var userData = (Build(providerConfiguration) as AppleOAuth2Client).GetUserDataDictionary(userAccessToken);
                userData["accesstoken"] = userAccessToken;
                var id = userData["sub"];
                var email = userData["email"];
                return new AuthenticationResult(true, this.ProviderName, id, email, userData);
            }
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel providerConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl) {
            if (previousAuthResult != null && previousAuthResult.IsSuccessful && !string.IsNullOrWhiteSpace(previousAuthResult.Provider)) {
                return previousAuthResult;
            }
            else {
                var client = BuildMobile(providerConfiguration) as AppleOAuth2Client;
                string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
                return GetUserData(providerConfiguration, previousAuthResult, userAccessToken);
            }
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            return clientData;
        }

        public bool RewriteRequest() {
            return RewriteRequestByPostState();
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

        private bool RewriteRequestByPostState() {
            bool result = false;
            var ctx = System.Web.HttpContext.Current;
            var stateString = System.Web.HttpUtility.HtmlDecode(ctx.Request.Form["state"]);
            if (stateString != null && stateString.Contains("__provider__=Apple")) {
                // this provider requires that all return data be packed into a "state" parameter
                var q = System.Web.HttpUtility.ParseQueryString(stateString);
                var codeString = System.Web.HttpUtility.HtmlDecode(ctx.Request.Form["code"]);
                if (!string.IsNullOrWhiteSpace(codeString)) {
                    var q2 = System.Web.HttpUtility.ParseQueryString(codeString);
                    q.Add("code", codeString);
                }
                q.Add(ctx.Request.QueryString);
                ctx.RewritePath(ctx.Request.Path + "?" + q.ToString());
                result = true;
            }
            return result;
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
            if (File.Exists(p8File)) {
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
            else {
                Logger.Error("OpenAuthentication - GetClientSecret - Apple Client is misconfigured. The file or path '{0}' is missing", p8File);
                return null;
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
    }
}