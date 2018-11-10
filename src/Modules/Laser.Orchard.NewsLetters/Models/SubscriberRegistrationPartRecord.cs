using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.NewsLetters.Models {
    public class SubscriberRegistrationPartRecord : ContentPartRecord {
        public virtual string NewsletterDefinitionIds { get; set; }
        public virtual bool PermitCumulativeRegistrations { get; set; }
    }
}