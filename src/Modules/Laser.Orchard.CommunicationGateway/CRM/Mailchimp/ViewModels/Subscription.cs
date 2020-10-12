using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Subscription {
        public Subscription() {
            Subscribed = true;
        }
        public Audience Audience { get; set; }
        public bool Subscribed { get; set; }
    }
}