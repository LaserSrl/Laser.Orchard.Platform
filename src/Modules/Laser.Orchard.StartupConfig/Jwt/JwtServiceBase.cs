using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.StartupConfig.Jwt
{
    public abstract class JwtServiceBase : IJwtService
    {
        protected readonly IOrchardServices _orchardServices;
        protected HttpClient WebApiClient { get; set; }
        protected JwtSecurityToken JwtToken { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public abstract void JwtLogin();
        public abstract void JwtTokenRenew();
        public abstract string GetBaseUrl();
        public abstract KeyValuePair<string, string> GetAuthHeader();
        public JwtServiceBase(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            WebApiClient = new HttpClient();
        }
        ~JwtServiceBase()
        {
            WebApiClient.Dispose();
        }
        private void EnsureJwtToken(double minutesBeforeExpirationForRenew = 10.0)
        {
            var now = DateTime.UtcNow;
            if (JwtToken != null)
            {
                var minutesToExpiration = (JwtToken.ValidTo - now).TotalMinutes;
                if (minutesToExpiration > 0 && minutesToExpiration <= minutesBeforeExpirationForRenew)
                {
                    JwtTokenRenew();
                }
            }
            if ((JwtToken == null) || (now > JwtToken.ValidTo))
            {
                JwtLogin();
            }
        }
        public string GetJwtToken() {
            EnsureJwtToken();
            if(JwtToken != null) {
                return JwtToken.RawData ?? "";
            }
            else {
                return "";
            }
        }
        protected CallResult ResultFromApiGet(string resource, string parameters = null)
        {
            var result = CallResult.Failure;
            try
            {
                EnsureJwtToken();
                var url = ComposeUrl(resource, parameters);
                if (JwtToken != null)
                {
                    //var authHeader = new AuthenticationHeaderValue("Bearer", JwtToken.RawData);
                    result = CallWebApi(url, GetAuthHeader(), HttpMethod.Get);
                }
            }
            catch
            {
                // return with Failure
            }
            return result;
        }
        protected CallResult ResultFromApiPost(string resource, string content, string queryStringParameters = null, string contentMimeType = "application/json", string responseMimeType = "application/json", int requestTimeoutMillis = 30000)
        {
            var result = CallResult.Failure;
            try
            {
                EnsureJwtToken();
                var url = ComposeUrl(resource, queryStringParameters);
                if (JwtToken != null)
                {
                    result = CallWebApi(url, GetAuthHeader(), HttpMethod.Post, content, contentMimeType, responseMimeType, requestTimeoutMillis);
                }
            }
            catch
            {
                // return with Failure
            }
            return result;
        }
        private string ComposeUrl(string resource, string urlParameters = null)
        {
            var url = GetBaseUrl().TrimEnd('/');
            if(string.IsNullOrWhiteSpace(resource) == false)
            {
                url = string.Format("{0}/{1}", url, resource);
            }
            if (urlParameters != null)
            {
                url += string.Format("?{0}", urlParameters);
            }
            return url;
        }

        protected CallResult CallWebApi(string url, KeyValuePair<string, string> authHeader, HttpMethod method, string content = null, string contentMimeType = "application/json", string responseMimeType = "application/json", int requestTimeoutMillis = 30000)
        {
            var result = CallResult.Failure;
            try
            {
                WebApiClient.DefaultRequestHeaders.Clear();
                WebApiClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                if (authHeader.Key != null)
                {
                    WebApiClient.DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
                }
                // call web api
                Task<HttpResponseMessage> t = null;
                if (method == HttpMethod.Get)
                {
                    t = WebApiClient.GetAsync(url);
                }
                else if (method == HttpMethod.Post)
                {
                    WebApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(responseMimeType));
                    ServicePointManager.Expect100Continue = false;
                    t = WebApiClient.PostAsync(url, new StringContent(content, Encoding.UTF8, contentMimeType));
                }
                if (t != null)
                {
                    if (t.Wait(requestTimeoutMillis)) {
                        if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {
                            var aux = t.Result.Content.ReadAsStringAsync();
                            aux.Wait();
                            result.Body = aux.Result;
                            if (t.Result.IsSuccessStatusCode) {
                                result.Success = true;
                            }
                            else {
                                Logger.Error("CallWebApi: Error {1} - {2} on request {0}.", url, (int)(t.Result.StatusCode), t.Result.ReasonPhrase);
                            }
                        }
                        else {
                            Logger.Error("CallWebApi: request {0} not completed and ended in status {1}.", url, t.Status);
                        }
                    }
                    else {
                        Logger.Error("CallWebApi: Timeout on request {0}. Timeout value (millis): {1}", url, requestTimeoutMillis);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CallWebApi error.");
            }
            return result;
        }
        protected class CallResult
        {
            public bool Success { get; set; }
            public string Body { get; set; }
            public static CallResult Failure
            {
                get
                {
                    return new CallResult() { Success = false, Body = "" };
                }
            }
        }
    }
}