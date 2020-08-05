using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Subscriber {
        public string Identifier { get; set; }
        public string Email_address { get; set; }
        public string Status { get; set; }
        public List<Tag> Tags { get; set; }
    }
}