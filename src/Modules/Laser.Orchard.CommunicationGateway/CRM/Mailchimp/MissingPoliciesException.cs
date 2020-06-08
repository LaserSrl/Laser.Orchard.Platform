using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp {
    [Serializable]
    public class MissingPoliciesException : Exception {
        public MissingPoliciesException() 
            : base("Not all required polices have been accepted.") {

        }
    }
}