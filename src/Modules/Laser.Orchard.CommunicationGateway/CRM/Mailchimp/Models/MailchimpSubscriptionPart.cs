using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Models {
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