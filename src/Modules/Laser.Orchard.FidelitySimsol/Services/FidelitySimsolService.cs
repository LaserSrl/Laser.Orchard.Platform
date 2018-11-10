using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Services;
using Orchard;
using Orchard.Security;
using Laser.Orchard.FidelityGateway.Models;
using Orchard.Data;
using Orchard.Workflows.Services;

namespace Laser.Orchard.FidelitySimsol.Services
{
    public class FidelitySimsolService : FidelityBaseServices
    {
        public FidelitySimsolService(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService, IRepository<ActionInCampaignRecord> repository, IWorkflowManager workfloManager)
            : base(orchardServices, encryptionService,
                authenticationService, membershipService,
                sendService, repository, workfloManager)
        {

        }

        public override string GetProviderName()
        {
            return "simsol";
        }

        //TODO controllare se con la campagna sbagliata crasha male
        public override APIResult<FidelityCampaign> GetCampaignData(string id)
        {
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = id;
            APIResult<FidelityCampaign> res = _sendService.SendCampaignData(settingsPart, campaign);
            APIResult<IEnumerable<FidelityCampaign>> resList = _sendService.SendCampaignList(settingsPart);
            FidelityCampaign campInList = resList.data.Where(c => c.Id.Equals(id)).First();
            res.data.Data = campInList.Data;
            res.data.Name = campInList.Name;
            return res;
        }
    }
}