using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Laser.Orchard.Policy.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Tokens;
using Orchard.Users.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpApiService : IMailchimpApiService {
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly IEncryptionService _encryptionService;
        private readonly IPolicyServices _policyServices;
        private readonly IMailchimpService _mailchimpService;
        private readonly IWorkflowManager _workflowManager;
        
        public MailchimpApiService(
            ShellSettings shellSettings,
            IOrchardServices orchardServices,
            IEncryptionService encryptionService,
            ITokenizer tokenizer,
            IPolicyServices policyServices,
            IMailchimpService mailchimpService,
            IWorkflowManager workflowManager) {

            _tokenizer = tokenizer;
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _policyServices = policyServices;
            _mailchimpService = mailchimpService;
            _workflowManager = workflowManager;
            
            Logger = NullLogger.Instance;

        }


        public ILogger Logger { get; set; }

        public Audience Audience(string id) {
            Audience audience = new Audience();
            HttpResponseMessage response = new HttpResponseMessage();
            var urlTokens = new Dictionary<string, string> {
                { "{list-id}", id}
            };
            string result = "";
            if (TryApiCall(HttpVerbs.Get, CalculateUrlByType(RequestTypes.List, urlTokens), null, ErrorHandlerDefault, ref result)) {
                audience = ToAudience(JObject.Parse(result));
            }
            return audience;
        }

        public List<Audience> Audiences() {
            List<Audience> audiences = new List<Audience>();
            string result = "";
            if (TryApiCall(HttpVerbs.Get, CalculateUrlByType(RequestTypes.Lists, null), null, ErrorHandlerDefault, ref result)) {
                audiences = ToAudiences(JObject.Parse(result));
            }
            return audiences;
        }

        public bool TryUpdateSubscription(MailchimpSubscriptionPart part, bool isUserCreation = false) {
            var sub = part.Subscription;
            if (sub.Audience == null || string.IsNullOrWhiteSpace(sub.Audience.Identifier)) return false;

            var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();
            var putPayload = _tokenizer.Replace(settings.PutPayload, new { Content = part.ContentItem });
            var memberEmail = _tokenizer.Replace(settings.MemberEmail, new { Content = part.ContentItem });

            var syncronized = false;
            var urlTokens = new Dictionary<string, string> {
                { "{list-id}",sub.Audience.Identifier},
                { "{member-id}",_mailchimpService.ComputeSubscriberHash(memberEmail) }
            };
            string result = "";
            var urlApiCall = CalculateUrlByType(RequestTypes.Member, urlTokens);
            if (sub.Subscribed) {
                // register member
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                JObject body = JObject.Parse(putPayload ?? "{}");
                syncronized = TryApiCall(HttpVerbs.Put, urlApiCall, body, ErrorHandlerDefault, ref result);
                if (isUserCreation) {
                    _workflowManager.TriggerEvent("UserCreatedOnMailchimp",
                        part,
                        () => new Dictionary<string, object> {
                            {"Syncronized", syncronized},
                            {"APICallEdit", new APICallEdit {
                                Url = urlApiCall,
                                RequestType = RequestTypes.Member.ToString(),
                                HttpVerb = HttpVerbs.Put.ToString(),
                                Payload = putPayload
                            }},
                            {"Email",part.As<UserPart>()== null ? body["email_address"].ToString() :  part.As<UserPart>().Email}
                        });
                } else {
                    _workflowManager.TriggerEvent("UserUpdatedOnMailchimp",
                        part,
                        () => new Dictionary<string, object> {
                            {"Syncronized", syncronized},
                            {"APICallEdit", new APICallEdit {
                                Url = urlApiCall,
                                RequestType = RequestTypes.Member.ToString(),
                                HttpVerb = HttpVerbs.Put.ToString(),
                                Payload = putPayload
                            }},
                            {"Email",part.As<UserPart>()== null ? body["email_address"].ToString() :  part.As<UserPart>().Email}
                        });
                }
            } else {
                // deleted member
                syncronized = TryApiCall(HttpVerbs.Delete, urlApiCall, null, ErrorHandlerDelete, ref result);
                _workflowManager.TriggerEvent("UserDeletedOnMailchimp",
                    part,
                    () => new Dictionary<string, object> {
                        {"Syncronized", syncronized},
                        {"APICallEdit", new APICallEdit {
                            Url = urlApiCall,
                            RequestType = RequestTypes.Member.ToString(),
                            HttpVerb = HttpVerbs.Delete.ToString(),
                            Payload = putPayload
                        }},
                        {"Email",part.As<UserPart>().Email}
                    });
            }
            return syncronized;
        }

        public bool TryApiCall(HttpVerbs httpVerb, string url, JObject bodyRequest, ref string result) {
            // assigned correct handler error
            Func<HttpVerbs, string, JObject, HttpResponseMessage, bool> errorHandler;
            if (httpVerb == HttpVerbs.Delete) {
                errorHandler = ErrorHandlerDelete;
            } else if (httpVerb == HttpVerbs.Get) {
                errorHandler = ErrorHandlerGetMember;
            } else {
                errorHandler = ErrorHandlerDefault;
            }

            return TryApiCall(httpVerb, url, bodyRequest, errorHandler, ref result);
        }

        private bool TryApiCall(HttpVerbs httpVerb, string url, JObject bodyRequest, Func<HttpVerbs, string, JObject, HttpResponseMessage, bool> ErrorHandler, ref string result) {
            var requestUrl = GetBaseUrl() + url;
            using (var httpClient = new HttpClient()) {
                SetHeader(httpClient);
                HttpResponseMessage response;
                if (httpVerb == HttpVerbs.Put) {
                    response = httpClient.PutAsJsonAsync(new Uri(requestUrl), bodyRequest).Result;
                } else if (httpVerb == HttpVerbs.Post) {
                    response = httpClient.PostAsJsonAsync(new Uri(requestUrl), bodyRequest).Result;
                } else if (httpVerb == HttpVerbs.Delete) {
                    response = httpClient.DeleteAsync(new Uri(requestUrl)).Result;
                } else if (httpVerb == HttpVerbs.Get) {
                    response = httpClient.GetAsync(new Uri(requestUrl), HttpCompletionOption.ResponseContentRead).Result;
                } else {
                    throw new Exception("Http verb not supported.");
                }
                result = response.Content.ReadAsStringAsync().Result;


                return ErrorHandler(httpVerb, requestUrl, bodyRequest, response);
            }

        }

        #region Delegate to handle errors
        public bool ErrorHandlerDefault(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response) {
            if (!response.IsSuccessStatusCode) {
                LogError(httpVerb, requestUrl, bodyRequest, response);
                return false;
            } else {
                return true;
            }
        }
        private void LogError(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response) {
            string errorMessage = "Mailchimp: Error while pushing data to mailchimp.\r\n" +
                                      "VERB: {0}\r\n" +
                                      "URL: {1}\r\n" +
                                      "Payload:\r\n{2}\r\n\r\n" +
                                      "Mailchimp Response:\r\n{3}\r\n";

            Logger.Error(errorMessage, httpVerb,
                                        requestUrl,
                                        bodyRequest,
                                        response.ReasonPhrase + "\r\n" + response.Content.ReadAsStringAsync().Result);
        }

        private void LogDebug(string Message,HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response) {
            string errorMessage = Message + "\r\n" +
                                      "VERB: {0}\r\n" +
                                      "URL: {1}\r\n" +
                                      "Payload:\r\n{2}\r\n\r\n" +
                                      "Mailchimp Response:\r\n{3}\r\n";

            Logger.Debug(errorMessage, httpVerb,
                                        requestUrl,
                                        bodyRequest,
                                        response.ReasonPhrase + "\r\n" + response.Content.ReadAsStringAsync().Result);
        }

        public bool ErrorHandlerDelete(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response) {
            if (!response.IsSuccessStatusCode) {
                if (response.StatusCode == HttpStatusCode.NotFound) {
                    // mail deleted not found in mailchimp
                    LogDebug("Mailchimp: The user's email was not found.", httpVerb, requestUrl, bodyRequest, response);
                    return true;
                } else if (response.StatusCode == HttpStatusCode.MethodNotAllowed) {
                    // mail is in the archive of mailchimp
                    LogDebug("Mailchimp: The user has an archived status.", httpVerb, requestUrl, bodyRequest, response);
                    return true;
                }
                LogError(httpVerb, requestUrl, bodyRequest, response);
                return false;
            } else {
                return true;
            }
        }

        public bool ErrorHandlerGetMember(HttpVerbs httpVerb, string requestUrl, JObject bodyRequest, HttpResponseMessage response) {
            if (!response.IsSuccessStatusCode) {
                LogError(httpVerb, requestUrl, bodyRequest, response);
                return false;
            } else {
                var resultJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                if(resultJson["status"] != null && resultJson["status"].ToString().ToLower() == StatusResponse.Archived.ToString().ToLower()) {                 
                    LogDebug("Mailchimp: The user has an archived status.", httpVerb, requestUrl, bodyRequest, response);
                    return false;
                }
                return true;
            }
        }
        #endregion 

        public List<RequestTypeInfo> GetRequestTypes() {
            return new List<RequestTypeInfo> {
                new RequestTypeInfo {
                    Type = RequestTypes.Member,
                    UrlTemplate = "lists/{list-id}/members/{member-id}", //DoNot change this template! theese fake tokens are used during url generation
                    PayloadTemplate = "\"email_address\": \"hermes.sbicego@laser1.com\", "+
                                        "\"status\": \"subscribed\","+
                                        "\"merge_fields\": {\"FNAME\": \"Hermes\","+
                                        "\"LNAME\": \"Sbicego2\"," +
                                        "\"ADDRESS\": \"Via Roma, 12\"," +
                                        "\"PHONE\": \"2222\"," +
                                        "\"BIRTHDAY\": \"03/22\"" +
                                    "}," +
                                    "\"language\":\"it\"," +
                                    "\"vip\": true"
                },
                new RequestTypeInfo {
                    Type = RequestTypes.Lists,
                    UrlTemplate ="lists/" },
                new RequestTypeInfo {
                    Type = RequestTypes.List,
                    UrlTemplate = "lists/{list-id}" },
                new RequestTypeInfo {
                    Type = RequestTypes.Tags,
                    UrlTemplate = "lists/{list-id}/members/{member-id}/tags",
                    PayloadTemplate = "\"tags\":[{{\r\n\"name\": \"tag1\", \"status\": \"active\"\r\n}},\r\n{{\r\n\"name\": \"tag2\", \"status\": \"inactive\"\r\n}}]" }
                };
        }

        public string GetBaseUrl() {
            var apiKey = _mailchimpService.DecryptApiKey();
            return string.Format("https://{0}.api.mailchimp.com/3.0/", apiKey.Substring(apiKey.LastIndexOf("-") + 1));
        }

        private string GetBase64AuthenticationToken() {
            var authenticationString = _shellSettings.Name + ":" + _mailchimpService.DecryptApiKey();
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
            return Convert.ToBase64String(textBytes);
        }

        private List<Audience> ToAudiences(JObject response) {
            var json = response;
            var audiences = new List<Audience>();
            var list = json["lists"];
            foreach (JObject item in list) {
                audiences.Add(ToAudience(item));
            }
            return audiences;
        }
        private Audience ToAudience(JObject jobject) {
            var json = jobject;
            return new Audience {
                Identifier = json["id"].Value<string>(),
                Name = json["name"].Value<string>()
            };
        }

        private void SetHeader(HttpClient client) {
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + GetBase64AuthenticationToken());
        }

        private string CalculateUrlByType(RequestTypes urlType, Dictionary<string, string> urlTokens) {
            var requestUrl = "";
            try {
                requestUrl += GetRequestTypes().First(x => x.Type == urlType).UrlTemplate;
            } catch {
                throw new Exception("Request Type missing.");
            }
            foreach (var token in urlTokens) {
                requestUrl = requestUrl.Replace(token.Key, token.Value);
            }
            return requestUrl;
        }

        public bool IsUserRegistered(MailchimpSubscriptionPart part) {
            string result = "";
            var urlTokens = new Dictionary<string, string> {
                        { "{list-id}", part.Subscription.Audience.Identifier},
                        { "{member-id}", _mailchimpService.ComputeSubscriberHash(part.ContentItem.As<UserPart>() != null ? part.ContentItem.As<UserPart>().Email : "") }
                    };
            return TryApiCall(HttpVerbs.Get, CalculateUrlByType(RequestTypes.Member, urlTokens), null, ErrorHandlerGetMember, ref result);
        }
    }
}