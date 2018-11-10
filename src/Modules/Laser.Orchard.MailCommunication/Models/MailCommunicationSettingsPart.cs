using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.Models {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationSettingsPart : ContentPart 
    {
        // Id Template Unsubscription
        public int? IdTemplateUnsubscribe {
            get { return this.Retrieve(x => x.IdTemplateUnsubscribe); }
            set { this.Store(x => x.IdTemplateUnsubscribe, value); }
        }
    }
}