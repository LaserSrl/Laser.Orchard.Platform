using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Activities {
    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public abstract class GDPRContentActivity : Event {

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {

                var contentTypesState = activityContext.GetState<string>("ContentTypes");

                // "" means 'any'
                if (String.IsNullOrEmpty(contentTypesState)) {
                    return true;
                }

                string[] contentTypes = contentTypesState.Split(',');

                var content = workflowContext.Content;

                if (content == null) {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
            } catch {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override string Form {
            get {
                // this form is defined in Orchard.Workflows.Forms.ContentForms.cs
                return "SelectContentTypes";
            }
        }

        public override LocalizedString Category {
            get { return T("Content Items"); }
        }
    }

    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class ContentAnonymizedActivity : GDPRContentActivity {
        public override string Name {
            get { return "ContentAnonymized"; }
        }

        public override LocalizedString Description {
            get { return T("Content is Anonymized. The GDPRContentContext token contains further information."); }
        }
    }

    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class ContentErasedActivity : GDPRContentActivity {
        public override string Name {
            get { return "ContentErased"; }
        }

        public override LocalizedString Description {
            get { return T("Content is Erased. The GDPRContentContext token contains further information."); }
        }
    }
}