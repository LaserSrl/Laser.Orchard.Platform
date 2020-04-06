using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

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