using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Activities {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class UserCreatedOnMailchimp : Event {
        // 3 different events created for the user
        //  - created
        //  - updated
        //  - deleted
        // based on the value of Syncronized I execute the return of success failure
        // the incoming call information and the user's email are there
        // I've separated them out for possible different reasoning for each one.
        public UserCreatedOnMailchimp() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public override string Name {
            get { return "UserCreatedOnMailchimp"; }
        }

        public override LocalizedString Description {
            get { return T("User registration on Mailchimp."); }
        }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override LocalizedString Category {
            get { return T("Mailchimp"); }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            bool syncronized = workflowContext.Tokens["Syncronized"] == null ? false : (bool)workflowContext.Tokens["Syncronized"];
            workflowContext.SetState("Syncronized", syncronized);
            workflowContext.SetState("APICallEdit", workflowContext.Tokens["APICallEdit"]);
            workflowContext.SetState("Email", workflowContext.Tokens["Email"]);
            if (syncronized) {
                messageout = T("Succeeded");
            } else {
                messageout = T("Failed");
            }
            yield return messageout;
        }
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Succeeded"), T("Failed") };
        }
    }
}