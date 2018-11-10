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
    public class AddPointsActivity : Event
    {
        public Localizer T { get; set; }

        public AddPointsActivity()
        {
            T = NullLocalizer.Instance;
        }

        public override bool CanStartWorkflow
        {
            get { return true; }
        }

        public override string Name
        {
            get
            {
                return "AddFidelityPoints";
            }
        }

        public override LocalizedString Category
        {
            get
            {
                return T("Fidelity");
            }
        }

        public override LocalizedString Description
        {
            get
            {
                return T("Manage Adding Fidelity Points");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Success"), T("Error") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            APIResult<CardPointsCampaign> res = (APIResult<CardPointsCampaign>)(workflowContext.Tokens["result"]);
            CardPointsCampaign card = res.data;

            if (res.success)
            {
                workflowContext.SetState<string>("customerId", card.idCustomer);
                workflowContext.SetState<string>("campaignId", card.idCampaign);
                workflowContext.SetState<double>("points", card.points);
                yield return T("Success");
            }
            else
            {
                yield return T("Error");
            }

        }

    }
}