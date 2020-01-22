using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.IdentityModel.Tokens;
using Orchard.Logging;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using DotNetOpenAuth.AspNet;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class AppleOAuth2Client : OAuth2Client {
        /// <summary>
        /// The keys endpoint.
        /// </summary>
        private const string _authorizationEndpoint = "https://appleid.apple.com/auth/authorize";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string _tokenEndpoint = "https://appleid.apple.com/auth/token";

        /// <summary>
        /// The base uri for scopes.
        /// </summary>
        private const string _scopeBaseUri = "https://appleid.apple.com";

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
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient;
        static AppleOAuth2Client() {
            _httpClient = new HttpClient();
        }
        public AppleOAuth2Client(string clientId, string clientSecret, ILogger logger)
            : base("Apple") {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _logger = logger;
        }
        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            return BuildUri(_authorizationEndpoint, new NameValueCollection
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
            // the input parameter is id_token because it contains user data and there is no need to make another call to Apple server
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
            postData.Add(new NameValueCollection {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
            });

            var response = MakePostRequest(_tokenEndpoint, postData.ToString());

            if ( ! string.IsNullOrWhiteSpace(response)) {
                var json = JObject.Parse(response);
                var accessToken = json.Value<string>("id_token");
                // returns id_token because it contains user data and GetUserData() extracts these informations from it
                return accessToken;
            }
            return null; // fallback
        }
        private string MakePostRequest(string url, string content) {
            var result = "";
            lock (_httpClient) {
                _httpClient.DefaultRequestHeaders.Clear();
                // specify to use TLS 1.2 as default connection if needed
                if (url.ToLowerInvariant().StartsWith("https:")) {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                // call web api
                Task<HttpResponseMessage> t = null;
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "krake");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                t = _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                if (t != null) {
                    t.Wait(3000); // timeout: 3 seconds
                    if (t.Status == TaskStatus.RanToCompletion) {
                        var aux = t.Result.Content.ReadAsStringAsync();
                        aux.Wait();
                        if (t.Result.IsSuccessStatusCode) {
                            result = aux.Result;
                        }
                        else {
                            _logger.Error("AppleOAuth2Client - MakePostRequest - Http Error {0} - {1} on request {2}.", (int)(t.Result.StatusCode), t.Result.ReasonPhrase, url);
                            throw new Exception(string.Format("AppleOAuth2Client - MakePostRequest: Http Error {0} - {1} on request {2}.", (int)(t.Result.StatusCode), t.Result.ReasonPhrase, url));
                        }
                    }
                    else {
                        _logger.Error("AppleOAuth2Client - MakePostRequest timeout on {0}", url);
                        throw new Exception(string.Format("AppleOAuth2Client - MakePostRequest: Timeout on request {0}.", url));
                    }
                }
                return result;
            }
        }
        public override AuthenticationResult VerifyAuthentication(HttpContextBase context, Uri returnPageUrl) {
            var code = context.Request.Form["code"];
            var accessToken = QueryAccessToken(returnPageUrl, code);
            var userData = GetUserData(accessToken);
            return new AuthenticationResult(true, ProviderName, userData["sub"], userData["email"], userData);
        }
    }
}