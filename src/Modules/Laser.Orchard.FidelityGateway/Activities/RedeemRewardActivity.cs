using Laser.Orchard.FidelityGateway.Models;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Activities
{
    public class RedeemRewardActivity : Event
    {
        public Localizer T { get; set; }

        public RedeemRewardActivity()
        {
            T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override string Name {
            get {
                return "RedeemFidelityReward";
            }
        }

        public override LocalizedString Category {
            get {
                return T("Fidelity");
            }
        }

        public override LocalizedString Description {
            get {
                return T("Manage Redeem Fidelity Reward");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Success"), T("Error")};
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            APIResult<FidelityReward> res = (APIResult<FidelityReward>)(workflowContext.Tokens["result"]);
            if (res.success)
            {
                FidelityReward rew = res.data;
                workflowContext.SetState<string>("rewardId", rew.Id);
                if (!string.IsNullOrWhiteSpace(rew.Description))
                {
                    workflowContext.SetState<string>("rewardDescription", rew.Description);
                }
                if (!string.IsNullOrWhiteSpace(rew.Name))
                {
                    workflowContext.SetState<string>("rewardName", rew.Name);
                }
                else {
                    workflowContext.SetState<string>("rewardName", "");
                }

                yield return T("Success");
            }
            else
            {
                yield return T("Error");
            }
            
        }
    }
}