using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models {
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

        public string DefaultAudience {
            get {
                return this.Retrieve<string>("DefaultAudience");
            }

            set {
                this.Store("DefaultAudience", value);
            }
        }
    }
}