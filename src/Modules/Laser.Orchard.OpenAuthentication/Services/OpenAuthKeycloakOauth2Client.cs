using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.OpenAuthentication.Services {
    [OrchardFeature("Laser.Orchard.OpenAuthentication.Keycloak")]
    public class OpenAuthKeycloakOauth2Client : OAuth2Client {
        // Keycloak implements the OpenID Connect specification:
        // https://openid.net/specs/openid-connect-core-1_0.html
        #region endpoints
        // These strings are used in string.Format() calls to have the actual endpoints
        // {0}: the "base url" of the keycloak IDP
        // {1}: the keycloak realm. This is Case sensitive

        // The well-known configuration endpoint returns an object that describes
        // the other endpoints.
        // TODO: use this endpoint to fetch the full configuration, rather than 
        // hardcoding only the portions we need.
        private const string WellKnownEndpointFormat = 
            "{0}/realms/{1}/.well-known/openid-configuration";
        private const string AuthorizationEndpointFormat =
            "{0}/realms/{1}/protocol/openid-connect/auth";
        private const string TokenEndpointFormat =
            "{0}/realms/{1}/protocol/openid-connect/token";
        private const string UserInfoEndpointFormat =
            "{0}/realms/{1}/protocol/openid-connect/userinfo";
        #endregion

        // A client_id is mandatory in most calls to the IDP
        private readonly string _clientId;

        private readonly string _idpUrl;
        private readonly string _realm;
        private readonly string[] _requestedScopes;
        // The specification has some mandatory scopes
        private static readonly string[] _mandatoryScopes = { "openid" };

        public OpenAuthKeycloakOauth2Client(
            string clientId, string idpUrl, string realm, params string[] requestedScopes)
            : base("Keycloak") {

            _clientId = clientId;
            _idpUrl = idpUrl;
            _realm = realm;
            _requestedScopes = _mandatoryScopes
                .Concat(requestedScopes ?? new string[] { })
                .Distinct()
                .ToArray()
                ;
        }

        protected OpenAuthKeycloakOauth2Client(string clientId, string idpUrl, string realm) 
            : this(clientId, idpUrl, realm, new[] { "email" }) { }

        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

            return OAuthHelpers.BuildUri(string.Format(AuthorizationEndpointFormat, _idpUrl, _realm), 
                new NameValueCollection
                {
                    { "client_id", _clientId },
                    { "response_type", "code" },
                    { "scope", string.Join(" ", _requestedScopes) },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "state", state },
                });
        }

        public IDictionary<string, string> GetUserDataDictionary(string accessToken) {
            return GetUserData(accessToken);
        }

        protected override IDictionary<string, string> GetUserData(string accessToken) {
            //var uri = OAuthHelpers.BuildUri(string.Format(UserInfoEndpointFormat, _idpUrl, _realm), 
            //    new NameValueCollection { { "access_token", accessToken } });
            var webRequest = (HttpWebRequest)WebRequest.Create(
                string.Format(UserInfoEndpointFormat, _idpUrl, _realm));
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {accessToken}");

            using (var webResponse = webRequest.GetResponse()) {
                using (var stream = webResponse.GetResponseStream()) {
                    if (stream != null) {
                        using (var textReader = new StreamReader(stream)) {
                            var json = textReader.ReadToEnd();
                            var extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            return extraData;
                        }
                    }
                }
            }
            return null;
        }

        public string GetAccessToken(Uri returnUrl, string authorizationCode) {
            return QueryAccessToken(returnUrl, authorizationCode);
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {

            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "client_id", _clientId },
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(
                string.Format(TokenEndpointFormat, _idpUrl, _realm));
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {authorizationCode}");
#if DEBUG
            // When debugging, also accept selfsigned certificates. We must do this
            // because TLS is mandatory for this.
            webRequest.ServerCertificateValidationCallback += 
                (sender, certificate, chain, sslPolicyErrors) => true;
#endif
            using (var s = webRequest.GetRequestStream()) {
                using (var sw = new StreamWriter(s)) {
                    sw.Write(postData.ToString());
                }
            }

            using (var webResponse = webRequest.GetResponse()) {
                using (var stream = webResponse.GetResponseStream()) {
                    if (stream != null) {
                        using (var reader = new StreamReader(stream)) {
                            var response = reader.ReadToEnd();
                            var json = JObject.Parse(response);
                            var accessToken = json.Value<string>("access_token");
                            return accessToken;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="returnPageUrl">The return URL which should match the value passed to RequestAuthentication() method.</param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult"/> containing authentication result.
		/// </returns>
		public override AuthenticationResult VerifyAuthentication(HttpContextBase context, Uri returnPageUrl) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            string code = context.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code)) {
                return AuthenticationResult.Failed;
            }

            string accessToken = this.QueryAccessToken(returnPageUrl, code);
            if (accessToken == null) {
                return AuthenticationResult.Failed;
            }

            IDictionary<string, string> userData = this.GetUserData(accessToken);
            if (userData == null) {
                return AuthenticationResult.Failed;
            }

            string name, id;
            if (!userData.TryGetValue("sub", out id)) {
                // the sub (subject) identifies the user
                return AuthenticationResult.Failed;
            }
            // Keycloak doesn't necessarily return a value for "username".
            if (!userData.TryGetValue("username", out name)
                && !userData.TryGetValue("preferred_username", out name)
                && !userData.TryGetValue("email", out name)) {
                // as a fall back, use the id
                name = id;
            }

            // add the access token to the user data dictionary: this is used in registration
            if (userData.ContainsKey("accesstoken")) {
                userData["accesstoken"] = accessToken;
            } else {
                userData.Add("accesstoken", accessToken);
            }

            return new AuthenticationResult(
                isSuccessful: true, 
                provider: this.ProviderName, 
                providerUserId: id, 
                userName: name, 
                extraData: userData);
        }
    }
}