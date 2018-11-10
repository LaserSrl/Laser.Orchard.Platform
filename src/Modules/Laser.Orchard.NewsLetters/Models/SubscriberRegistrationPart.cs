using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.Models {
    public class SubscriberRegistrationPart : ContentPart<SubscriberRegistrationPartRecord> {
        public string NewsletterDefinitionIds {
            get { return Record.NewsletterDefinitionIds; }
            set { Record.NewsletterDefinitionIds = value; }
        }
        public bool PermitCumulativeRegistrations {
            get { return Record.PermitCumulativeRegistrations; }
            set { Record.PermitCumulativeRegistrations = value; }
        }
    }
}