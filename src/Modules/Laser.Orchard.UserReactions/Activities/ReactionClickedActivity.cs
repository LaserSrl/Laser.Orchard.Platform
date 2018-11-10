using Orchard.Localization;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Activities {
    public class ReactionClickedActivity : Event {
        public Localizer T { get; set; }

        public ReactionClickedActivity() {
            T = NullLocalizer.Instance;
        }
        public override string Name {
            get { return "ReactionClicked"; }
        }
        public override bool CanStartWorkflow {
            get {
                return true;
            }
        }
        public override LocalizedString Description {
            get { return T("A user reaction is clicked. Sets the following values in workflow state: ReactionId, Action, ReactionUserEmail, ReactionUserId."); }
        }
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(global::Orchard.Workflows.Models.WorkflowContext workflowContext, global::Orchard.Workflows.Models.ActivityContext activityContext) {
            return new[] { T("Clicked"), T("Unclicked"), T("NothingToDo") };
        }
        public override LocalizedString Category {
            get {
                return T("Social");
            }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = null;
            var elencoTypeId = ((string)activityContext.GetState<string>("ReactionClickedActivity_reactionTypes")).Split(',').Select(Int32.Parse).ToList();
            int reactionId = Convert.ToInt32(workflowContext.Tokens["ReactionId"]);
            int action = Convert.ToInt32(workflowContext.Tokens["Action"]);
            int userId = Convert.ToInt32(workflowContext.Tokens["UserId"]);
            workflowContext.SetState<int>("ReactionId", reactionId);
            workflowContext.SetState<int>("Action", action);
            workflowContext.SetState<string>("ReactionUserEmail", workflowContext.Tokens["UserEmail"].ToString());
            workflowContext.SetState<int>("ReactionUserId", userId);
            if (elencoTypeId.Contains(reactionId)) {
                if (action == 1) {
                    messageout = T("Clicked");
                }
                else {
                    messageout = T("Unclicked");
                }
            }
            else {
                messageout = T("NothingToDo");
            }
            yield return messageout;
        }
        public override string Form {
            get {
                return "TheFormReactionClicked";
            }
        }

    }
}