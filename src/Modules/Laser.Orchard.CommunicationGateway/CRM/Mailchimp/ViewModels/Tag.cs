using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Tag {
        public int Identifier { get; set; }
        public string Name { get; set; }
    }
}