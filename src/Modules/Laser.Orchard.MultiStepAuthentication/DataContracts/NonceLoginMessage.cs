using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.DataContracts {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    [DataContract]
    public class NonceLoginMessage {
        [DataMember(Name = "nonce")]
        public string Nonce { get; set; }
    }
}