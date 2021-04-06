using Laser.Orchard.NwazetIntegration.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Activities {
    public class ValidateAttributesActivity : Event {
        public const string EventName = "ValidateAttributes";

        public ValidateAttributesActivity() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name => EventName;

        public override LocalizedString Category => T("Commerce");

        public override LocalizedString Description =>
            T("A user is attempting to add to cart a product with attributes.");

        public override bool CanStartWorkflow => true;

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            // copy workflow tokens in workflow state so they are 
            // accessible by {Workflow.State:*}
            workflowContext.SetState("Context",
                workflowContext.Tokens.ContainsKey("Context")
                    ? (AttributesValidationContext)workflowContext.Tokens["Context"]
                    : (AttributesValidationContext)null);
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }
    }
}