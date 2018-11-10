using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Activities {
    public abstract class OpenAuthUserActivity : Event {
        protected OpenAuthUserActivity() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Events"); }
        }
    }
    
    public class OpenAuthUserCreatingActivity : OpenAuthUserActivity {
        public override string Name {
            get { return "OpenAuthUserCreating"; }
        }

        public override LocalizedString Description {
            get { return T("Open Auth User is creating."); }
        }
    }

    public class OpenAuthUserCreatedActivity : OpenAuthUserActivity {
        public override string Name {
            get { return "OpenAuthUserCreated"; }
        }

        public override LocalizedString Description {
            get { return T("Open Auth User is created."); }
        }
    }

    public class OpenAuthRecordCreatedActivity : OpenAuthUserActivity {
        public override string Name {
            get { return "OpenAuthRecordCreated"; }
        }

        public override LocalizedString Description {
            get { return T("Open Auth UserProviderRecord is created."); }
        }
    }

    public class OpenAuthRecordUpdatedActivity : OpenAuthUserActivity {
        public override string Name {
            get { return "OpenAuthRecordUpdated"; }
        }

        public override LocalizedString Description {
            get { return T("Open Auth UserProviderRecord is updated."); }
        }
    }
}