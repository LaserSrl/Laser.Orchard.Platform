using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Web.Script.Serialization;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPart : ContentPart {

        public Subscription Subscription {
            get {
                return DeserializeSubscription(this.Retrieve<string>("Subscription"));
            }
            set {
                this.Store("Subscription", SerializeSubscription(value));
            }
        }

        private Subscription DeserializeSubscription(string Subscription) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            if (Subscription != null) {
                return serializer.Deserialize<Subscription>(Subscription);
            }
            else {
                return new Subscription();
            } 
                
        }

        private string SerializeSubscription(Subscription Subscription) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(Subscription ?? new Subscription());
        }
    }
}