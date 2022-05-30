using Laser.Orchard.StartupConfig.ContentSync.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync.Activities {
    public class SyncContentActivity : Task {
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;
        private readonly ISyncService _syncService;

        public SyncContentActivity(IContentManager contentManager,
                                   ISyncService syncService) {
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
            _syncService = syncService;
        }



        public override string Name {
            get {
                return "SyncContent";
            }
        }

        public override string Form {
            get {
                return "SyncContentForm";
            }
        }

        public override LocalizedString Category {
            get {
                return T("Content");
            }
        }

        public override LocalizedString Description {
            get {
                return T("Sync the content with another");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Success"), T("Error") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = T("Success");
            try {
                var context = new SyncContext {
                    Source = workflowContext.Content.ContentItem, //TODO: Source should be choosen by Form tokenized field
                    Target = new Target {
                        Type = activityContext.GetState<string>("TargetType"),
                        EnsureCreating = activityContext.GetState<bool>("Creating"),
                        EnsurePublishing = activityContext.GetState<bool>("Publishing"),
                        EnsureVersioning = activityContext.GetState<bool>("Versioning"),
                        ForceOwnerUpdate = activityContext.GetState<bool>("ForceOwnerUpdate")
                    }
                };
                _syncService.Synchronize(context);
                //TODO: Add the result ContentItem as WorkflowState and Token in order to have it in next steps;
            }
            catch { messageout = T("Error"); }
            yield return messageout;
        }
    }
}