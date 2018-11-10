using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Models;

namespace Laser.Orchard.FidelityGateway.ViewModels
{
    public class EditAction
    {
        public virtual int Id { get; set; }
        public virtual string Action { get; set; }
        public virtual double Points { get; set; }
        public virtual string CampaignId { get; set; }
        public virtual string Provider { get; set; }

        public List<string> CampaignList;

        public EditAction()
        {
            Action = "";
            Points = 0;
            CampaignId = "";
            Provider = "";
        }

    }
}