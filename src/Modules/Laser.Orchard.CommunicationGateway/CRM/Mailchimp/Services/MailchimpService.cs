using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Security;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpService : IMailchimpService {
        private readonly IOrchardServices _orchardServices;
        private readonly IEncryptionService _encryptionService;
        private readonly IPolicyServices _policyService;
        private readonly IControllerContextAccessor _controllerAccessor;

        public MailchimpService(ShellSettings shellSettings,
            IOrchardServices orchardServices,
            IEncryptionService encryptionService,
            ITokenizer tokenizer,
            IPolicyServices policyService,
            IControllerContextAccessor controllerAccessor) {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _policyService = policyService;
            _controllerAccessor = controllerAccessor;
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

        public void CheckAcceptedPolicy(MailchimpSubscriptionPart part) {
            //Check if all policies have been accepted before firing a Mailchimp member update
            var policies = new List<PoliciesForUserViewModel>();
            var settings = part.Settings.GetModel<MailchimpSubscriptionPartSettings>();

            IEnumerable<PolicyAnswer> allAnswers = TryFindPolicyAnswers(part);

            if (// sanity checks
                part.Subscription != null
                && part.Subscription.Audience != null
                && !string.IsNullOrWhiteSpace(part.Subscription.Audience.Identifier)
                // and only try to check stuff if the user subscribed
                && part.Subscription.Subscribed) {

                if (settings.PolicyTextReferencesToArray() != null && settings.PolicyTextReferencesToArray().Any()) {
                    var accepted = allAnswers.Where(x => x.Accepted).Select(x => "{" + x.PolicyTextId + "}").ToList();
                    var requiredPolicies = settings.PolicyTextReferencesToArray().OrderBy(x => x).ToList();
                    var missingPoliciesIds = requiredPolicies.Except(accepted);
                    if (missingPoliciesIds.Any()) { // If Required Policies have not been accepted
                        var ids = missingPoliciesIds.Select(x => int.Parse(x.Trim(new char[] { '{', '}' }))).ToArray();
                        // GET the policies with the current culture
                        // TODO: make the culture check optional via settings
                        var localizedMissingPoliciesids = _orchardServices.ContentManager.GetMany<CommonPart>(ids, VersionOptions.Published, QueryHints.Empty).Where(x => x.As<LocalizationPart>() == null ||
                            (x.As<LocalizationPart>() != null
                            && x.As<LocalizationPart>().Culture != null
                            && x.As<LocalizationPart>().Culture.Culture == _orchardServices.WorkContext.CurrentCulture))
                            .Select(x => x.Id);
                        if (localizedMissingPoliciesids.Count() > 0) {
                            throw new MissingPoliciesException();
                        }
                    }
                }
            }
            
        }

        public string ComputeSubscriberHash(string input) {
            // Mailchimp expect the input to be lowercase
            input = input.ToLower(CultureInfo.InvariantCulture);
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

        private IEnumerable<PolicyAnswer> TryFindPolicyAnswers(MailchimpSubscriptionPart part) {
            // Read Answers from the DB
            var answers = _policyService.GetPolicyAnswersForContent(part.Id).Select(x => new PolicyAnswer {
                PolicyTextId = x.PolicyTextInfoPartRecord.Id,
                Accepted = x.Accepted
            }).ToList();
            // Adds Current answers if existing
            if (_controllerAccessor.Context != null && _controllerAccessor.Context.Controller.ViewBag.PolicyAnswers != null) {
                var currentAnswers = (List<PolicyAnswer>)_controllerAccessor.Context.Controller.ViewBag.PolicyAnswers;
                foreach (var answer in currentAnswers) { //Removes previous answers to same Policy
                    answers.RemoveAll((match) => {
                        return (match.PolicyTextId == answer.PolicyTextId);
                    });
                }
                answers = answers.Union(currentAnswers).ToList(); //Merge new answers
            }
            return answers;
        }
    }
}