using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
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


            if (settings.PolicyTextReferences != null && settings.PolicyTextReferences.Any()) {
                var accepted = allAnswers.Where(x => x.Accepted).Select(x => "{" + x.PolicyTextId + "}").ToList();
                var requiredPolicies = settings.PolicyTextReferences.OrderBy(x => x).ToList();
                var missingPoliciesIds = requiredPolicies.Except(accepted);
                if (missingPoliciesIds.Any()) { // If Required Policies have not been accepted
                    throw new MissingPoliciesException();
                }
            }
        }

        private IEnumerable<PolicyAnswer> TryFindPolicyAnswers(MailchimpSubscriptionPart part) {
            // Read Answers from the DB
            var answers = _policyService.GetPolicyAnswersForContent(part.Id).Select(x=>new PolicyAnswer {
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