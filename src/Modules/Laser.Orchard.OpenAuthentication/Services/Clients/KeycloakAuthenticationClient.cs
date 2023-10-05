using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    [OrchardFeature("Laser.Orchard.OpenAuthentication.Keycloak")]
    public class KeycloakAuthenticationClient : IExternalAuthenticationClient {

        private Dictionary<string, LocalizedString> _attributeKeys;
        public Localizer T { get; set; }

        public KeycloakAuthenticationClient() {

            T = NullLocalizer.Instance;

            _attributeKeys = new Dictionary<string, LocalizedString>();
            _attributeKeys.Add("idpUrl", T("Base URL of the Identity provider."));
            _attributeKeys.Add("realm", T("Name of the realm (case sensitive)."));
        }

        public string ProviderName {
            get { return "Keycloak"; }
        }
       
        public Dictionary<string, LocalizedString> GetAttributeKeys() {
            return _attributeKeys;
        }

        public IAuthenticationClient Build(ProviderConfigurationViewModel providerConfiguration) {
            string ClientId = providerConfiguration.ProviderIdKey;
            var idpUrl = providerConfiguration
                   .Attributes.FirstOrDefault(at => "idpUrl".Equals(at.AttributeKey))?.AttributeValue;
            var realm = providerConfiguration
                   .Attributes.FirstOrDefault(at => "realm".Equals(at.AttributeKey))?.AttributeValue;

            //TODO: handle null values for these params
            var client = new OpenAuthKeycloakOauth2Client(ClientId, idpUrl, realm);
            return client;
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken) {
            var userData = (Build(clientConfiguration) as OpenAuthKeycloakOauth2Client)
                .GetUserDataDictionary(userAccessToken);
            userData["accesstoken"] = userAccessToken;
            string id = userData["id"];
            string name = userData["email"];
            userData["name"] = userData["email"];
            return new AuthenticationResult(true, this.ProviderName, id, name, userData);
        }

        public AuthenticationResult GetUserData(ProviderConfigurationViewModel clientConfiguration, AuthenticationResult previousAuthResult, string token, string userAccessSecretKey, string returnUrl) {
            var client = Build(clientConfiguration) as OpenAuthKeycloakOauth2Client;
            string userAccessToken = client.GetAccessToken(new Uri(returnUrl), token);
            return GetUserData(clientConfiguration, previousAuthResult, userAccessToken);
        }

        public OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData) {
            OpenAuthCreateUserParams retVal = clientData;
            string emailAddress = string.Empty;
            foreach (KeyValuePair<string, string> values in clientData.ExtraData) {
                if (values.Key == "mail") {
                    retVal.UserName = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                }
            }
            return retVal;
        }

        public bool RewriteRequest() {
            return new ServiceUtility().RewriteRequestByState();
        }
    }
}