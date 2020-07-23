using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.ContactForm.Activity {
    public class ContactFormSubmittedEvent : Event {
        protected readonly IOrchardServices _orchardServices;
        public ContactFormSubmittedEvent(IOrchardServices orchardServices) {
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
            try {

                var identityValues = activityContext.GetState<string>("IdentityValues");

                // "" means 'any'
                if (String.IsNullOrEmpty(identityValues)) {
                    return true;
                }

                string[] contentIdentities = identityValues.Split(',');

                var content = workflowContext.Content;

                if (content == null) {
                    return false;
                }

                return contentIdentities.Any(identity => _orchardServices.ContentManager.GetItemMetadata(content).Identity.ToString() == identity);
            }
            catch {
                return false;
            }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Marketing"); }
        }
        public override string Name {
            get {
                return "ContactFormSubmittedEvent";
            }
        }
        public override string Form {
            get { return "_ContactFormSubmittedForm"; }
        }

        public override LocalizedString Description {
            get {
                return T("A contact form is submitted"); 
            }
        }
    }
}

