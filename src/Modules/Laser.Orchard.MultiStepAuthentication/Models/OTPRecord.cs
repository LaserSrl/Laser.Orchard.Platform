using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Models {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class OTPRecord {
        public virtual int Id { get; set; } // Primary Key

        public virtual UserPartRecord UserRecord { get; set; }

        [StringLengthMax]
        public virtual string Password { get; set; } // could be a Nonce, the pwd used for OTP...

        /// <summary>
        /// The type of the password for this record. Basically this is used to discriminate among 
        /// different systems/providers, e.g. OTP, Nonce...
        /// </summary>
        public virtual string PasswordType { get; set; }

        /// <summary>
        /// Depending on the specific implementation of multi-step authentication, this string will
        /// contain different things. It may be a device UUID, or a json with more complex information.
        /// Each implementation will have to take care to handle its own things.
        /// </summary>
        [StringLengthMax]
        public virtual string AdditionalData { get; set; }

        public virtual DateTime ExpirationUTCDate { get; set; }
    }
}