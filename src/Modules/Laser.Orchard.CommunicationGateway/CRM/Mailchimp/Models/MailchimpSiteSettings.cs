using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Models {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteSettings : ContentPart {

        public string ApiKey {
            get {
                return this.Retrieve<string>("ApiKey");
            }

            set {
                this.Store("ApiKey", value);
            }
        }
    }
}