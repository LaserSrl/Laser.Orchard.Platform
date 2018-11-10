using Laser.Orchard.CommunicationGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationCampaignPartDriver : ContentPartDriver<CommunicationCampaignPart> {
    }
}
//            private readonly IOrchardServices _orchardServices;

//        public ILogger Logger { get; set; }
//        public Localizer T { get; set; }

//        protected override string Prefix {
//            get { return "Laser.Orchard.CommunicationGateway"; }
//        }

//        public CommunicationCampaignPartDriver(IOrchardServices orchardServices) {
//            _orchardServices = orchardServices;
//            Logger = NullLogger.Instance;
//            T = NullLocalizer.Instance;
//        }
//        protected override DriverResult Editor(CommunicationCampaignPart part, dynamic shapeHelper) {
//            return null;
//        }



//        protected override DriverResult Editor(CommunicationCampaignPart part, IUpdateModel updater, dynamic shapeHelper) {
//            DateTime datepublish = ((DateTime)(((dynamic)part).ContentItem.PublishLaterPart.ScheduledPublishUtc.Value));
//            DateTime datelimitcampaign = (DateTime)((((dynamic)part).ToDate.DateTime));
//            if (datepublish > datelimitcampaign) {
//                throw new Exception("Date in publish later is over date campaign");
//            }
//            return null;
//        }
    
    
//    }
//}