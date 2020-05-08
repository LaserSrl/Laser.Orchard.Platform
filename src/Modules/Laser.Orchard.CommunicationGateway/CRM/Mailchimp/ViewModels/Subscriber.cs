using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Subscriber {
        public string Identifier { get; set; }
        public string Email_address { get; set; }
        public string Status { get; set; }
        public List<Tag> Tags { get; set; }
    }
}