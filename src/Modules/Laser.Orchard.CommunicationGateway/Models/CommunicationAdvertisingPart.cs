using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.CommunicationGateway.Models {
    public class CommunicationAdvertisingPart : ContentPart<CommunicationAdvertisingPartRecord> {
        public int CampaignId {
            get { return this.Retrieve(x=>x.CampaignId); }
            set { this.Store(x => x.CampaignId, value); }
        }
    }

    public class CommunicationAdvertisingPartRecord : ContentPartRecord {
        public virtual int CampaignId { get; set; }
    }
}