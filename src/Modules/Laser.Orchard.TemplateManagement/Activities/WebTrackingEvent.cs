using Orchard.Workflows.Services;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Activities {
    [OrchardFeature("Laser.Orchard.WebTracking")]
    public class WebTrackingEvent : Event {
        public Localizer T { get; set; }
        public WebTrackingEvent() {
            T = NullLocalizer.Instance;
        }
        public override bool CanStartWorkflow {
            get { return true; }
        }
        public override LocalizedString Category {
            get { return T("Content Event"); }
        }

        public override LocalizedString Description {
            get { return T("A user has just loaded a tracking image."); }
        }

        public override string Name {
            get { return "WebTrackingEvent"; }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }
    }
}