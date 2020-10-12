using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class APICallEdit {
        public string Payload { get; set; }
        public string RequiredPolicies { get; set; }
        public string HttpVerb { get; set; }
        public string RequestType { get; set; }
        public string Url { get; set; }
    }
}