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

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class AppleAuthenticationClient : IExternalAuthenticationClient {
        private readonly ShellSettings _shellSetting;
        public string ProviderName => "Apple";

        public AppleAuthenticationClient(ShellSettings shellSetting) {
            _shellSetting = shellSetting;
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            string clientId = providerConfigurationRecord.ProviderIdKey;
            string clientSecret = GetClientSecret(providerConfigurationRecord);
            var client = new AppleOAuth2Client(clientId, clientSecret);
            return client;
        }

        private string GetClientSecret(ProviderConfigurationRecord providerConfigurationRecord) {
            var epoch = new DateTime(1970, 1, 1);
            var payload = new Dictionary<string, object>() {
                { "iss", providerConfigurationRecord.UserIdentifier }, // team_id
                { "iat", (DateTime.UtcNow.AddMinutes(-1) - epoch).TotalSeconds },
                { "exp", (DateTime.UtcNow.AddMonths(5) - epoch).TotalSeconds },
                { "aud", "https://appleid.apple.com" },
                { "sub", providerConfigurationRecord.ProviderIdKey }, // client_id
            };
            var extraHeader = new Dictionary<string, object>() {
                { "alg", "ES256" },
                { "typ", "JWT" },
                { "kid", providerConfigurationRecord.ProviderIdentifier } // key_id
            };
            var p8File = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\" + providerConfigurationRecord.ProviderSecret;
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
            var userData = (Build(clientConfiguration) as AppleOAuth2Client).GetUserDataDictionary(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            var id = userData["sub"];
            var email = userData["email"];
            return new AuthenticationResult(true, this.ProviderName, id, email, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl) {
            var client = Build(clientConfiguration) as AppleOAuth2Client;
            string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            return GetUserData(clientConfiguration, previousAuthResult, userAccessToken);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            return clientData;
        }

        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
    }
}