using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Workflows {
    public class UserTrackingWorkflow : Event {
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;
        public UserTrackingWorkflow(IContentManager contentManager) {
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }



        public override string Name {
            get {
                return "UserTracking";
                //    throw new NotImplementedException(); 
            }
        }

        //public override string Form {
        //    get {
        //        return "UserTrackingWorkflowForm";
        //    }
        //}

        public override LocalizedString Category {
            get {
                return T("Social");// throw new NotImplementedException();
            }
        }

        public override LocalizedString Description {
            get {
                return T("Start on user interaction with contentitem having TrackingPart");
            }
        }
        public override bool CanStartWorkflow {
            get {
                return true;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Interaction") };
        }


        /// <summary>
        /// usando un token es: {Workflow.State:text} recupero il valore
        /// </summary>
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = T("Interaction");
            string text = workflowContext.Tokens["text"].ToString();
            string sourceType = workflowContext.Tokens["sourceType"].ToString();
            int count = Convert.ToInt32(workflowContext.Tokens["count"]);
            int UserId = Convert.ToInt32(workflowContext.Tokens["UserId"]);
            workflowContext.SetState<int>("UserId", UserId);
            workflowContext.SetState<int>("count", count);
            workflowContext.SetState<string>("sourceType", sourceType);
            workflowContext.SetState<string>("text", text);
            yield return messageout;
        }
    }
}

