using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class PinterestOAuth2Client : OAuth2Client {
        #region Constants and Fields

        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        /// 
        private const string AuthorizationEndpoint = "https://api.pinterest.com/oauth/";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://api.pinterest.com/v1/oauth/token";

        /// <summary>
        /// The user info endpoint.
        /// </summary>
        private const string UserInfoEndpoint = "https://api.pinterest.com/v1/me/";

        //private const string UserProfileFields = ":(id,first-name,last-name,headline,location:(name),industry,summary,picture-url,email-address,phone-numbers,main-address)";
        private const string UserProfileFields = "";
        /// <summary>
        /// The base uri for scopes.
        /// </summary>
        private const string ScopeBaseUri = "";

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
        private readonly string[] _requestedScopes = {"read_public"};

        #endregion

        public PinterestOAuth2Client(string clientId, string clientSecret) : base("Pinterest") {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public IDictionary<string, string> GetUserDataDictionary(string accessToken) {
            return GetUserData(accessToken);
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            var scopes = _requestedScopes.Select(x => !x.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? ScopeBaseUri + x : x);

            return BuildUri(AuthorizationEndpoint, new NameValueCollection
                {
                    { "response_type", "code" },
                    { "client_id", _clientId },
                    { "scope", string.Join(",", scopes) },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "state", returnUrl.Query.Substring(1) },
                });
        }

        protected override IDictionary<string, string> GetUserData(string accessToken) {
            var uri = BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", accessToken } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var json = textReader.ReadToEnd();
                    var valori = JsonConvert.DeserializeObject<PinterestUserData>(json);
                    var extraData = valori.data;
                    return extraData;
                }
            }
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret }
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(TokenEndpoint);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

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
                    var accessToken = json.Value<string>("access_token");
                    return accessToken;
                }
            }
        }

        public static Uri BuildUri(string baseUri, NameValueCollection queryParameters) {
            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
            var qs = String.Join("&", keyValuePairs);

            var builder = new UriBuilder(baseUri) { Query = qs };
            return builder.Uri;
        }
    }
}