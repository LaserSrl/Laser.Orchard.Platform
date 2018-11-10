using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class ActionInCampaignRecord
    {
        public virtual int Id { get; set; }
        public virtual string Action { get; set; }
        public virtual double Points { get; set; }
        public virtual string CampaignId { get; set; }
        public virtual string Provider { get; set; }
    }
}