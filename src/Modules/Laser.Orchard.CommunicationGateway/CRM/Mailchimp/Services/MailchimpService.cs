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
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp;
using Orchard.Data;
using Laser.Orchard.UsersExtensions.Models;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Services {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpService : IMailchimpService {
        private readonly IOrchardServices _orchardServices;
        private readonly IEncryptionService _encryptionService;

        public MailchimpService(ShellSettings shellSettings, IOrchardServices orchardServices, IEncryptionService encryptionService, ITokenizer tokenizer) {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
        }

        public string DecryptApiKey() {
            var settings = _orchardServices.WorkContext.CurrentSite.As<MailchimpSiteSettings>();
            var apiKey = _encryptionService.Decode(Convert.FromBase64String(settings.ApiKey ?? ""));
            return Encoding.UTF8.GetString(apiKey);
        }

        public string CryptApiKey(string apikey) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<MailchimpSiteSettings>();
            return Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(apikey)));
        }

        public void CheckAcceptedPolicy(MailchimpSubscriptionPart part, UserRegistrationPolicyPart policyPart) {
            //Check if all policies have been accepted before firing a Mailchimp member update
            var policies = new List<PoliciesForUserViewModel>();
            var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();
            if (settings.PolicyTextReferences != null && settings.PolicyTextReferences.Any()) {
                var allAnswers = policyPart.PolicyAnswers;
                var accepted = allAnswers.Where(x => x.Accepted).Select(x => "{" + x.PolicyTextId + "}").ToList();
                var requiredPolicies = settings.PolicyTextReferences.OrderBy(x => x).ToList();
                var missingPoliciesIds = requiredPolicies.Except(accepted);
                if (missingPoliciesIds.Any()) { // If Required Policies have not been accepted
                    throw new MissingPoliciesException();
                }
            }
        }
    }
}