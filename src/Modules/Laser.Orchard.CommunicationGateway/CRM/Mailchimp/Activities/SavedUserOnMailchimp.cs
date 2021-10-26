using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Activities {
    public class SavedUserOnMailchimp : Event {
        public SavedUserOnMailchimp() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public override string Name {
            get { return "UserSavedOnMailchimp"; }
        }

        public override LocalizedString Description {
            get { return T("User saved on Mailchimp."); }
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
            bool syncronized = workflowContext.Tokens["syncronized"] == null ? false : (bool)workflowContext.Tokens["syncronized"];
            workflowContext.SetState("syncronized", syncronized);
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