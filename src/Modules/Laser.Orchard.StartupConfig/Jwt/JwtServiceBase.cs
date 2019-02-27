using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.IdentityModel.Tokens;
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
        protected CallResult ResultFromApiGet(string resource, string parameters = null)
        {
            var result = CallResult.Failure;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            if (JwtToken != null)
            {
                var authHeader = new AuthenticationHeaderValue("Bearer", JwtToken.RawData);
                result = CallWebApi(url, authHeader, HttpMethod.Get);
            }
            return result;
        }
        protected CallResult ResultFromCaligooApiPost(string resource, string content, string parameters = null)
        {
            var result = CallResult.Failure;
            EnsureJwtToken();
            var url = ComposeUrl(resource, parameters);
            if (JwtToken != null)
            {
                var authHeader = new AuthenticationHeaderValue("Bearer", JwtToken.RawData);
                result = CallWebApi(url, authHeader, HttpMethod.Post, content);
            }
            return result;
        }
        private string ComposeUrl(string resource, string urlParameters = null)
        {
            var url = string.Format("{0}/{1}", GetBaseUrl().TrimEnd('/'), resource);
            if (urlParameters != null)
            {
                url += string.Format("?{0}", urlParameters);
            }
            return url;
        }

        protected CallResult CallWebApi(string url, AuthenticationHeaderValue auth, HttpMethod method, string content = null, int requestTimeoutMillis = 30000)
        {
            var result = CallResult.Failure;
            try
            {
                WebApiClient.DefaultRequestHeaders.Clear();
                if (auth != null)
                {
                    WebApiClient.DefaultRequestHeaders.Authorization = auth;
                }
                // specify to use TLS 1.2 as default connection if needed
                if (url.ToLowerInvariant().StartsWith("https:"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                // call web api
                Task<HttpResponseMessage> t = null;
                if (method == HttpMethod.Get)
                {
                    t = WebApiClient.GetAsync(url);
                }
                else if (method == HttpMethod.Post)
                {
                    WebApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    t = WebApiClient.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                }
                if (t != null)
                {
                    t.Wait(requestTimeoutMillis);
                    if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        var aux = t.Result.Content.ReadAsStringAsync();
                        aux.Wait();
                        result.Body = aux.Result;
                        if (t.Result.IsSuccessStatusCode)
                        {
                            result.Success = true;
                        }
                        else
                        {
                            Logger.Error("CallWebApi: Error {1} - {2} on request {0}.", url, (int)(t.Result.StatusCode), t.Result.ReasonPhrase);
                        }
                    }
                    else
                    {
                        Logger.Error("CallWebApi: Timeout on request {0}.", url);
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