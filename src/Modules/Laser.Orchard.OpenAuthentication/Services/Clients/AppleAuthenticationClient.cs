using System;
using System.IdentityModel.Tokens;
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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Security.Cryptography;

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
            var payload = new Dictionary<string, object>() {
                { "iss", providerConfigurationRecord.UserIdentifier }, // team_id
                { "iat", EpochTime.GetIntDate(DateTime.UtcNow.AddMinutes(-1)) },
                { "exp", EpochTime.GetIntDate(DateTime.UtcNow.AddMonths(5)) },
                { "aud", "https://appleid.apple.com" },
                { "sub", providerConfigurationRecord.ProviderIdKey }, // client_id
            };
            var extraHeader = new Dictionary<string, object>() {
                { "alg", "ES256" },
                { "typ", "JWT" },
                { "kid", providerConfigurationRecord.ProviderIdentifier } // key_id
            };
            var p8File = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\" + providerConfigurationRecord.ProviderSecret;
            //var keyString = File.ReadAllText(p8File, Encoding.UTF8); //the content of my .p8 downloaded private key, when I created the key in https://developer.apple.com/account/ios/authkey/create";
            //CspParameters cspParam = new CspParameters();
            //cspParam.Flags = CspProviderFlags.UseMachineKeyStore;
            //var rsa = new CngProvider(""); // RSACryptoServiceProvider(cspParam);
            // TODO: cambiare gestione leggendo direttamente la chiave già estratta dal p8
            //CngKey privateKey = CngKey.Import(Convert.FromBase64String(keyString), CngKeyBlobFormat.Pkcs8PrivateBlob);

            CngKey privateKey = GetPrivateKey(p8File);

            // version 1
            //string token = JWT.Encode(payload, privateKey, JwsAlgorithm.ES256, extraHeader);
            //return token;

            // version 2
            var headerString = JsonConvert.SerializeObject(extraHeader);
            var payloadString = JsonConvert.SerializeObject(payload);
            using (ECDsaCng dsa = new ECDsaCng(privateKey)) {
                dsa.HashAlgorithm = CngAlgorithm.Sha256;
                var unsignedJwtData = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerString)) + "." + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadString));
                var signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
                return unsignedJwtData + "." + Base64UrlEncoder.Encode(signature);
            }
        }
        /// <summary>
        /// Extracts private key from p8 file.
        /// </summary>
        /// <param name="p8File">Full path of the p8 file.</param>
        /// <returns></returns>
        private static CngKey GetPrivateKey(string p8File) {
            using (var reader = File.OpenText(p8File)) {
                var ecPrivateKeyParameters = (ECPrivateKeyParameters)new PemReader(reader).ReadObject();
                var x = ecPrivateKeyParameters.Parameters.G.AffineXCoord.GetEncoded();
                var y = ecPrivateKeyParameters.Parameters.G.AffineYCoord.GetEncoded();
                var d = ecPrivateKeyParameters.D.ToByteArrayUnsigned();
                return  EccKey.New(x, y, d);
            }
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