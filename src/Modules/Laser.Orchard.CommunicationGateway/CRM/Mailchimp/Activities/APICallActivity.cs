using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Activities {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class APICallActivity : Task {
        private readonly IMailchimpService _service;
        private readonly IMailchimpApiService _apiservice;

        public Localizer T { get; set; }

        public APICallActivity(IMailchimpService service, IMailchimpApiService apiService) {
            _service = service;
            _apiservice = apiService;
            T = NullLocalizer.Instance;
        }
        public override LocalizedString Category {
            get { return T("Mailchimp"); }
        }
        public override LocalizedString Description {
            get { return T("Calls Mailchimp API"); }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            string result = "";
            HttpVerbs verb = HttpVerbs.Get;
            var model = new APICallEdit {
                Url = activityContext.GetState<string>("Url"),
                RequestType = activityContext.GetState<string>("RequestType"),
                HttpVerb = activityContext.GetState<string>("HttpVerb"),
                Payload = activityContext.GetState<string>("Payload").ToString(),
                RequiredPolicies = activityContext.GetState<string>("RequiredPolicies")
            };
            Enum.TryParse<HttpVerbs>(model.HttpVerb.ToString(), true, out verb);
            JObject payload = JObject.Parse("{" + model.Payload + "}");
            var done = _apiservice.TryApiCall(verb, model.Url, payload, ref result);
            if (done) {
                messageout = T("Succeeded");
            }
            else {
                messageout = T("Failed");
            }
            yield return messageout;
        }
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Succeeded"), T("Failed") };
        }
        public override string Name {
            get { return "MailchimpApiCall"; }
        }

        public override string Form => "MailchimpAPICallForm";

    }
}