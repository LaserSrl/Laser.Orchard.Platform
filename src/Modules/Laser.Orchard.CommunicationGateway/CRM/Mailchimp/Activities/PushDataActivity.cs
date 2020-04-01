using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Activities {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class PushDataActivity : Event {
        private readonly IMailchimpService _service;
        private readonly IMailchimpApiService _apiservice;

        public Localizer T { get; set; }

        public PushDataActivity(IMailchimpService service, IMailchimpApiService apiService) {
            _service = service;
            _apiservice = apiService;
            T = NullLocalizer.Instance;
        }
        public override LocalizedString Category {
            get { return T("Mailchimp"); }
        }
        public override LocalizedString Description {
            get { return T("Push Data to Mailchimp CRM"); }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            //_apiservice.TryApiCall();
            var done = true;
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
            get { return "PushData"; }
        }

        public override bool CanStartWorkflow {
            get { return false; }
        }

        public override string Form => "PushDataForm";

    }
}