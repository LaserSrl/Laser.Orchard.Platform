using Orchard;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ContactForm.Activity {
    public class ContactFormValidating : Event {
        protected readonly IOrchardServices _orchardServices;
        public ContactFormValidating(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public Localizer T { get; set; }
        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Marketing"); }
        }
        public override string Name {
            get {
                return "ContactFormValidating";
            }
        }

        public override LocalizedString Description {
            get {
                return T("Validation of additional contact form fields");
            }
        }
    }
}