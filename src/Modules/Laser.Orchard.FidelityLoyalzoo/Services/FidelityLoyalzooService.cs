using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Services;
using Orchard;
using Orchard.Security;
using Orchard.Data;
using Laser.Orchard.FidelityGateway.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.FidelityLoyalzoo.Services
{
    public class FidelityLoyalzooService : FidelityBaseServices
    {
        public FidelityLoyalzooService(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService, IRepository<ActionInCampaignRecord> repository, IWorkflowManager workfloManager)
            : base(orchardServices, encryptionService,
                authenticationService, membershipService,
                sendService, repository, workfloManager)
        {
                    if (settingsPart.AccountID == null || settingsPart.DefaultCampaign == null)
                    {
                        try
                        {
                            otherSettings();
                        }
                        catch
                        {
                            //TODO ??
                        }           
                    }
       }

        public override string GetProviderName()
        {
            return "loyalzoo";
        }

        private void otherSettings()//TODO come fare per non metterlo nel costruttore??
        {
            APIResult<IDictionary<string, string>> res = _sendService.GetOtherSettings(settingsPart);
            if (res.success)
            {
                settingsPart.AccountID = res.data["merchantId"];
                settingsPart.DefaultCampaign = res.data["placeId"];
            }

        }
    }
}