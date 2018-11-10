using Laser.Orchard.PaymentGateway.Models;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Activities {
    public class PaymentEventActivity : Event {
        public Localizer T { get; set; }

        public PaymentEventActivity() {
            T = NullLocalizer.Instance;
        }
        public override LocalizedString Category {
            get { return T("Payment"); }
        }
        public override LocalizedString Description {
            get { return T("Manage payment result"); }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            PaymentRecord payment = (PaymentRecord)(workflowContext.Tokens["Payment"]);
            workflowContext.SetState<PaymentRecord>("Payment", payment);
            if (payment.Success) {
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
            get { return "PaymentEnded"; }
        }
        public override bool CanStartWorkflow {
            get { return true; }
        }
    }
}