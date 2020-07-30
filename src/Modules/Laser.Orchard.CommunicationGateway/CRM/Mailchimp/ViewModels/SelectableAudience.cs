using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class SelectableAudience {
        public Audience Audience { get; set; }
        public bool Selected { get; set; }
        public string[] RequiredPolicies { get ; set; }
    }
}