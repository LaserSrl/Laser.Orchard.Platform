using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Models;

namespace Laser.Orchard.FidelityGateway.ViewModels
{
    public class ActionCampaign
    {
        public List<ActionInCampaignRecord> actions {set; get;}
        public string usedProvider;
    }
}