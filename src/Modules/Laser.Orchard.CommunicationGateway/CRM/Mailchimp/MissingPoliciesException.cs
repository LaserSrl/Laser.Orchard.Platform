using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp {
    [Serializable]
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MissingPoliciesException : Exception {
        public MissingPoliciesException() 
            : base("Not all required polices have been accepted.") {

        }
    }
}