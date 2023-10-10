using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using Laser.Orchard.OpenAuthentication.Extensions;
using DotNetOpenAuth.AspNet;
using System.Data;
using System.Text;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class FacebookOAuth2Client :  OAuth2Client {
        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";
        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";
        /// <summary>
        /// The user info endpoint.
        /// </summary>
        private const string UserInfoEndpoint = "https://graph.facebook.com/me";
        
        /// <summary>
        /// The app id.
        /// </summary>
        private readonly string _appId;
        /// <summary>
        /// The app secret.
        /// </summary>
        private readonly string _appSecret;

        /// <summary>
        /// The requested scopes.
        /// </summary>
        private readonly string[] _requestedScopes;


        /// <summary>
        /// Creates a new Facebook OAuth2 client, requesting the default "email" scope.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        public FacebookOAuth2Client(string appId, string appSecret) : this(appId, appSecret, new[] { "email" }) { }

        /// <summary>
        /// Creates a new Facebook OAuth2 client.
        /// </summary>
        /// <param name="appId">The Facebook App Id</param>
        /// <param name="appSecret">The Facebook App Secret</param>
        /// <param name="requestedScopes">One or more requested scopes, passed without the base URI.</param>
        public FacebookOAuth2Client(string appId, string appSecret, params string[] requestedScopes) : base("facebook")
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException("appId");

            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException("appSecret");

            if (requestedScopes == null)
                throw new ArgumentNullException("requestedScopes");

            if (requestedScopes.Length == 0)
                throw new ArgumentException("One or more scopes must be requested.", "requestedScopes");

            _appId = appId;
            _appSecret = appSecret;
            _requestedScopes = requestedScopes;
        }

        public override void RequestAuthentication(HttpContextBase context, Uri returnUrl) {
            string redirectUrl = this.GetServiceLoginUrl(returnUrl).AbsoluteUri;
            context.Response.Redirect(redirectUrl, endResponse: true);
        }

        public new AuthenticationResult VerifyAuthentication(HttpContextBase context) {
            throw new NoNullAllowedException();
        }

        public override AuthenticationResult VerifyAuthentication(HttpContextBase context, Uri returnPageUrl) {
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

            string id = userData["id"];
            string name;

            // Some oAuth providers do not return value for the 'username' attribute. 
            // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
            if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
                name = id;
            }

            // add the access token to the user data dictionary just in case page developers want to use it
            userData["accesstoken"] = accessToken;

            return new AuthenticationResult(
                isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

            return OAuthHelpers.BuildUri(AuthorizationEndpoint, new NameValueCollection
                {
                    { "client_id", _appId },
                    { "scope", string.Join(" ", _requestedScopes) },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                    { "state", state },
                });
        }

        protected override IDictionary<string, string> GetUserData(string accessToken) {
            var uri = OAuthHelpers.BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", accessToken }, { "fields","id,email,birthday,first_name,last_name,name,locale,link,gender,timezone,updated_time,verified" } });
            
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream()) {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream)) {
                    var json = textReader.ReadToEnd();
                    var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var data = extraData.ToDictionary(x => x.Key, x => x.Value.ToString());

                    data.Add("picture", string.Format("https://graph.facebook.com/{0}/picture", data["id"]));
                    data.Add("username",data["email"].IsEmailAddress()? data["email"].Substring(0, data["email"].IndexOf('@')) : data["email"] );
                   
                        //            return null;
                        //        var userData = new Dictionary<string, string>();
                        //        userData["id"] = graphData.Id;
                        //        userData["username"] = graphData.Email.IsEmailAddress() ? graphData.Email.Substring(0, graphData.Email.IndexOf('@')) : graphData.Email; 
                        //        userData["name"] = graphData.Name;
                        //        userData["link"] = graphData.Link == null ? null : graphData.Link.AbsoluteUri;
                        //        userData["gender"] = graphData.Gender;
                        //        userData["birthday"] = graphData.Birthday;
                        //        userData["email"] = graphData.Email;

                        //        if (userData == null) {
                        //            //return AuthenticationResult.Failed;
                        //            return null;
                        //        }

                        //        string id = userData["id"];
                        //        string name;

                        //        // Some oAuth providers do not return value for the 'username' attribute. 
                        //        // In that case, try the 'name' attribute. If it's still unavailable, fall back to 'id'
                        //        if (!userData.TryGetValue("username", out name) && !userData.TryGetValue("name", out name)) {
                        //            name = id;
                        //        }

                        //        // add the access token to the user data dictionary just in case page developers want to use it
                        //        userData["accesstoken"] = accessToken;
                        return data;
                }
            }
        }

        public string QueryAccessTokenByCode(Uri returnUrl, string authorizationCode) {
            return this.QueryAccessToken(returnUrl, authorizationCode);
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
            var uri = OAuthHelpers.BuildUri(TokenEndpoint, new NameValueCollection
                {
                    { "code", authorizationCode },
                    { "client_id", _appId },
                    { "client_secret", _appSecret },
                    { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
                });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            string accessToken = null;
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            // handle response from FB 
            // this will not be a url with params like the first request to get the 'code'
            Encoding rEncoding = Encoding.GetEncoding(response.CharacterSet);

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), rEncoding)) {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var jsonObject = serializer.DeserializeObject(sr.ReadToEnd());
                var jConvert = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(jsonObject));

                Dictionary<string, object> desirializedJsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jConvert.ToString());
                accessToken = desirializedJsonObject["access_token"].ToString();
            }
            return accessToken;
        }


        /// <summary>
        /// Facebook works best when return data be packed into a "state" parameter.
        /// This should be called before verifying the request, so that the url is rewritten to support this.
        /// </summary>
        public static void RewriteRequest() {
            var ctx = HttpContext.Current;

            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString == null || !stateString.Contains("__provider__=facebook"))
                return;

            var q = HttpUtility.ParseQueryString(stateString);
            q.Add(ctx.Request.QueryString);
            q.Remove("state");

            ctx.RewritePath(ctx.Request.Path + "?" + q);
        }
    }
    //    #region Constants and Fields

    //    /// <summary>
    //    /// The authorization endpoint.
    //    /// </summary>
    //    private const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";

    //    /// <summary>
    //    /// The token endpoint.
    //    /// </summary>
    //    private const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";

    //    /// <summary>
    //    /// The user info endpoint.
    //    /// </summary>
    //    private const string UserInfoEndpoint = "https://graph.facebook.com/me";

    //    /// <summary>
    //    /// The app id.
    //    /// </summary>
    //    private readonly string _appId;

    //    /// <summary>
    //    /// The app secret.
    //    /// </summary>
    //    private readonly string _appSecret;

    //    /// <summary>
    //    /// The requested scopes.
    //    /// </summary>
    //    private readonly string[] _requestedScopes;

    //    #endregion

    //    /// <summary>
    //    /// Creates a new Facebook OAuth2 client, requesting the default "email" scope.
    //    /// </summary>
    //    /// <param name="appId">The Facebook App Id</param>
    //    /// <param name="appSecret">The Facebook App Secret</param>
    //    public FacebookOAuth2Client(string appId, string appSecret)
    //        : this(appId, appSecret, new[] { "email" }) { }

    //    /// <summary>
    //    /// Creates a new Facebook OAuth2 client.
    //    /// </summary>
    //    /// <param name="appId">The Facebook App Id</param>
    //    /// <param name="appSecret">The Facebook App Secret</param>
    //    /// <param name="requestedScopes">One or more requested scopes, passed without the base URI.</param>
    //    public FacebookOAuth2Client(string appId, string appSecret, params string[] requestedScopes)
    //        : base("facebook")
    //    {
    //        if (string.IsNullOrWhiteSpace(appId))
    //            throw new ArgumentNullException("appId");

    //        if (string.IsNullOrWhiteSpace(appSecret))
    //            throw new ArgumentNullException("appSecret");

    //        if (requestedScopes == null)
    //            throw new ArgumentNullException("requestedScopes");

    //        if (requestedScopes.Length == 0)
    //            throw new ArgumentException("One or more scopes must be requested.", "requestedScopes");

    //        _appId = appId;
    //        _appSecret = appSecret;
    //        _requestedScopes = requestedScopes;
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="returnUrl"></param>
    //    /// <returns></returns>
    //    protected override Uri GetServiceLoginUrl(Uri returnUrl)
    //    {
    //        var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);

    //        return BuildUri(AuthorizationEndpoint, new NameValueCollection
    //            {
    //                { "client_id", _appId },
    //                { "scope", string.Join(" ", _requestedScopes) },
    //                { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
    //                { "state", state },
    //            });
    //    }

    //    public 

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="accessToken"></param>
    //    /// <returns></returns>
    //    protected override IDictionary<string, string> GetUserData(string accessToken)
    //    {

    //        var serializer = new DataContractJsonSerializer(typeof(FacebookGraphData));
    //        FacebookGraphData graphData;
    //        var request =
    //            WebRequest.Create(
    //                "https://graph.facebook.com/me?fields=id,email,birthday,first_name,last_name,name,locale,link,gender,timezone,updated_time,verified&access_token=" + accessToken);
    //        try {
    //            using (var response = request.GetResponse()) {
    //                using (var responseStream = response.GetResponseStream()) {

    //                    graphData = JsonConvert.DeserializeObject<FacebookGraphData>(responseStream.ToString());
    //                   // (FacebookGraphData)serializer.ReadObject(responseStream);
    //                }
    //            }
    //        } catch {
    //           // return AuthenticationResult.Failed;
    //            return null;
    //        }
    //        // this dictionary must contains 
   

    //        return userData;

    //    }



    //    protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
    //    {
    //        var uri = BuildUri(TokenEndpoint, new NameValueCollection
    //            {
    //                { "code", authorizationCode },
    //                { "client_id", _appId },
    //                { "client_secret", _appSecret },
    //                { "redirect_uri", returnUrl.GetLeftPart(UriPartial.Path) },
    //            });

    //        var webRequest = (HttpWebRequest) WebRequest.Create(uri);

    //        using (var webResponse = webRequest.GetResponse())
    //        {
    //            var responseStream = webResponse.GetResponseStream();
    //            if (responseStream == null)
    //                return null;

    //            using (var reader = new StreamReader(responseStream))
    //            {
    //                var response = reader.ReadToEnd();

    //                var results = HttpUtility.ParseQueryString(response);
    //                return results["access_token"];
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Facebook works best when return data be packed into a "state" parameter.
    //    /// This should be called before verifying the request, so that the url is rewritten to support this.
    //    /// </summary>
    //    public static void RewriteRequest()
    //    {
    //        var ctx = HttpContext.Current;

    //        var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
    //        if (stateString == null || !stateString.Contains("__provider__=facebook"))
    //            return;

    //        var q = HttpUtility.ParseQueryString(stateString);
    //        q.Add(ctx.Request.QueryString);
    //        q.Remove("state");

    //        ctx.RewritePath(ctx.Request.Path + "?" + q);
    //    }
    //}
}