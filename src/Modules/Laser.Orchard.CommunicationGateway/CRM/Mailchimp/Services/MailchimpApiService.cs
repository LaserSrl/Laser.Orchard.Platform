using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Laser.Orchard.Policy.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public MailchimpApiService(
            ShellSettings shellSettings,
            IOrchardServices orchardServices,
            IEncryptionService encryptionService,
            ITokenizer tokenizer,
            IPolicyServices policyServices,
            IMailchimpService mailchimpService) {

            _tokenizer = tokenizer;
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _policyServices = policyServices;
            _mailchimpService = mailchimpService;

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
            if (TryApiCall(HttpVerbs.Get, CalculateUrlByType(RequestTypes.List, urlTokens), null, ref result)) {
                audience = ToAudience(JObject.Parse(result));
            }
            return audience;
        }

        public List<Audience> Audiences() {
            List<Audience> audiences = new List<Audience>();
            string result = "";
            if (TryApiCall(HttpVerbs.Get, CalculateUrlByType(RequestTypes.Lists, null), null, ref result)) {
                audiences = ToAudiences(JObject.Parse(result));
            }
            return audiences;
        }

        public bool TryUpdateSubscription(MailchimpSubscriptionPart part) {
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
            if (sub.Subscribed) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                JObject body = JObject.Parse(putPayload ?? "{}");
                syncronized = TryApiCall(HttpVerbs.Put, CalculateUrlByType(RequestTypes.Member, urlTokens), body, ref result);
            }
            else {
                syncronized = TryApiCall(HttpVerbs.Delete, CalculateUrlByType(RequestTypes.Member, urlTokens), null, ref result);
            }

            return syncronized;
        }


        public bool TryApiCall(HttpVerbs httpVerb, string url, JObject bodyRequest, ref string result) {
            var requestUrl = GetBaseUrl() + url;
            var syncronized = false;
            using (var httpClient = new HttpClient()) {
                SetHeader(httpClient);
                HttpResponseMessage response;
                if (httpVerb == HttpVerbs.Put) {
                    response = httpClient.PutAsJsonAsync(new Uri(requestUrl), bodyRequest).Result;
                }
                else if (httpVerb == HttpVerbs.Post) {
                    response = httpClient.PostAsJsonAsync(new Uri(requestUrl), bodyRequest).Result;
                }
                else if (httpVerb == HttpVerbs.Delete) {
                    response = httpClient.DeleteAsync(new Uri(requestUrl)).Result;
                }
                else if (httpVerb == HttpVerbs.Get) {
                    response = httpClient.GetAsync(new Uri(requestUrl), HttpCompletionOption.ResponseContentRead).Result;
                }
                else {
                    throw new Exception("Http verb not supported.");
                }
                result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode) {
                    syncronized = false;
                    string errorMessage = "Mailchimp: Error while pushing data to mailchimp.\r\n" +
                                            "VERB: {0}\r\n" +
                                            "URL: {1}\r\n" +
                                            "Payload:\r\n{2}\r\n\r\n" +
                                            "Mailchimp Response:\r\n{3}\r\n";

                    Logger.Error(errorMessage, httpVerb,
                                                requestUrl,
                                                bodyRequest,
                                                response.ReasonPhrase + "\r\n" + result);
                }
                else {
                    syncronized = true;
                }

                return syncronized;
            }

        }

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
            }
            catch {
                throw new Exception("Request Type missing.");
            }
            foreach (var token in urlTokens) {
                requestUrl = requestUrl.Replace(token.Key, token.Value);
            }
            return requestUrl;
        }
    }
}