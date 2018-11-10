using Laser.Orchard.Vimeo.Extensions;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Activities {
    public abstract class VimeoVideoActivity : Event {

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {
                
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public override LocalizedString Category {
            get { return T("Vimeo"); }
        }
    }

    public class VimeoVideoPublishedActivity : VimeoVideoActivity {
        public override string Name {
            get { return Constants.PublishedSignalName; }
        }

        public override LocalizedString Description {
            get { return T("Vimeo video has been published."); }
        }
    }
}