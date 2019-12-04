using System;
using System.IdentityModel.Tokens;
using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Jose;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public class AppleAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName => "Apple";

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
            var keyString = providerConfigurationRecord.ProviderSecret; //the content of my .p8 downloaded private key, when I created the key in https://developer.apple.com/account/ios/authkey/create";
            CngKey privateKey = CngKey.Import(Convert.FromBase64String(keyString), CngKeyBlobFormat.Pkcs8PrivateBlob);
            string token = JWT.Encode(payload, privateKey, JwsAlgorithm.ES256, extraHeader);
            return token;
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