using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.IdentityModel.Tokens;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class AppleOAuth2Client : OAuth2Client {
        /// <summary>
        /// The keys endpoint.
        /// </summary>
        private const string AuthorizationEndpoint = "https://appleid.apple.com/auth/authorize";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://appleid.apple.com/auth/token";

        /// <summary>
        /// The base uri for scopes.
        /// </summary>
        private const string ScopeBaseUri = "https://appleid.apple.com";

        /// <summary>
        /// The _app id.
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// The _app secret.
        /// </summary>
        private readonly string _clientSecret;

        /// <summary>
        /// The requested scopes.
        /// </summary>
        private readonly string _requestedScopes = "name email";

        public AppleOAuth2Client(string clientId, string clientSecret)
            : base("Apple") {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }
        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            return BuildUri(AuthorizationEndpoint, new NameValueCollection
                {
                    { "response_type", "code" },
                    { "response_mode", "form_post" },
                    { "client_id", _clientId },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "state", returnUrl.Query.Substring(1) },
                    { "scope", _requestedScopes },
                });
        }
        public static Uri BuildUri(string baseUri, NameValueCollection queryParameters) {
            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
            var qs = String.Join("&", keyValuePairs);
            var builder = new UriBuilder(baseUri) { Query = qs };
            return builder.Uri;
        }
        public IDictionary<string, string> GetUserDataDictionary(string accessToken) {
            return GetUserData(accessToken);
        }

        protected override IDictionary<string, string> GetUserData(string accessToken) {
            var userData = new Dictionary<string, string>();
            var jwtHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)jwtHandler.ReadToken(accessToken);
            foreach(var claim in securityToken.Claims) {
                userData.Add(claim.Type, claim.Value);
            }
            return userData;
        }

        public string GetAccessToken(Uri returnUrl, string authorizationCode) {
            return QueryAccessToken(returnUrl, authorizationCode);
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(TokenEndpoint);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";
            webRequest.UserAgent = "krake"; // Apple requires a user agent header at the token endpoint

            using (var s = webRequest.GetRequestStream())
            using (var sw = new StreamWriter(s))
                sw.Write(postData.ToString());

            using (var webResponse = webRequest.GetResponse()) {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                    return null;

                using (var reader = new StreamReader(responseStream)) {
                    var response = reader.ReadToEnd();
                    var json = JObject.Parse(response);
                    var accessToken = json.Value<string>("id_token"); // json.Value<string>("access_token");
                    return accessToken;
                }
            }
        }
    }
}