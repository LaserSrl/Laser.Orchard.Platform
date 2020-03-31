using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Orchard.Environment.Extensions;
using Orchard.Tokens;
using System.Web.Script.Serialization;
using System.Linq;
using Laser.Orchard.Policy.Services;
using Orchard.Logging;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
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
            using (var httpClient = new HttpClient()) {
                SetHeader(httpClient);
                var response = httpClient.GetAsync(new Uri(GetListUrl(id)), HttpCompletionOption.ResponseContentRead).Result;

                if (response.IsSuccessStatusCode) {
                    audience = ToAudience(response.Content.ReadAsAsync<dynamic>().Result);
                }
            }
            return audience;
        }

        public List<Audience> Audiences() {
            List<Audience> audiences = new List<Audience>();
            using (var httpClient = new HttpClient()) {
                SetHeader(httpClient);
                var response = httpClient.GetAsync(new Uri(GetListsUrl()), HttpCompletionOption.ResponseContentRead).Result;

                if (response.IsSuccessStatusCode) {
                    audiences = ToAudiences(response.Content.ReadAsAsync<dynamic>().Result);
                }
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
            using (var httpClient = new HttpClient()) {
                HttpResponseMessage response;
                SetHeader(httpClient);
                if (sub.Subscribed) {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    object body = serializer.Deserialize<dynamic>(putPayload ?? "{}");
                    response = httpClient.PutAsJsonAsync(new Uri(GetMemberUrl(sub.Audience.Identifier, memberEmail)), body).Result;
                }
                else {
                    response = httpClient.DeleteAsync(new Uri(GetMemberUrl(sub.Audience.Identifier, memberEmail))).Result;
                }
                if (!response.IsSuccessStatusCode) {
                    syncronized = false;
                    string errorMessage = "Mailchimp: Error while syncronyzing member.\r\n" +
                                            "Error while {0} {1} to Mailchimp.\r\n" +
                                            "Payload:\r\n{2}\r\n\r\n" +
                                            "Mailchimp Response:\r\n{3}\r\n";
                    
                    Logger.Error(errorMessage, sub.Subscribed ? "Subscribing" : "Unsubscribing",
                                                memberEmail,
                                                putPayload,
                                                response.ReasonPhrase + "\r\n" + response.Content.ReadAsStringAsync().Result);
                }
                else {
                    syncronized = true;
                }

                return syncronized;
            }
        }

        private string GetBaseUrl() {
            var apiKey = _mailchimpService.DecryptApiKey();
            return string.Format("https://{0}.api.mailchimp.com/3.0/", apiKey.Substring(apiKey.LastIndexOf("-") + 1));
        }

        private string GetBase64AuthenticationToken() {
            var authenticationString = _shellSettings.Name + ":" + _mailchimpService.DecryptApiKey();
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
            return Convert.ToBase64String(textBytes);
        }

        private string GetMemberUrl(string listId, string memberEmail) {
            return GetListsUrl() + listId + "/members/" + CalculateMD5Hash(memberEmail) + "/";
        }

        private string GetListUrl(string listId) {
            return GetBaseUrl() + "lists/" + listId + "/";
        }
        private string GetListsUrl() {
            return GetBaseUrl() + "lists/";
        }

        private List<Audience> ToAudiences(object response) {
            var json = (JObject)response;
            var audiences = new List<Audience>();
            var list = json["lists"];
            foreach (var item in list) {
                audiences.Add(ToAudience(item));
            }
            return audiences;
        }
        private Audience ToAudience(object jobject) {
            var json = (JObject)jobject;
            return new Audience {
                Identifier = json["id"].Value<string>(),
                Name = json["name"].Value<string>()
            };
        }

        private string CalculateMD5Hash(string input) {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
        private void SetHeader(HttpClient client) {
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + GetBase64AuthenticationToken());
        }

    }
}