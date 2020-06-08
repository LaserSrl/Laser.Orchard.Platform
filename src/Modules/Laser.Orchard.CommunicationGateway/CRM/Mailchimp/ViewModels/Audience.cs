using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Audience {
        public string Identifier { get; set; }
        public string Name { get; set; }
    }
}