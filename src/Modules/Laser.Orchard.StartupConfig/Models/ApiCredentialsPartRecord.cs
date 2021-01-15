using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsPartRecord : ContentPartRecord {

        public virtual string Key { get; set; }

        public virtual string Secret { get; set; }
        
        // TODO: we want to be able, even in the future, to have a user
        // retrieve their key and secret. This means that we should store
        // it in some reversible form, either plain (not) or encrypted (yes).
        // We also want to store the secret's hash for quicker comparisons
        // when we authenticate using these credentials.
        // Hence, here we'll need a bunch of additional fields/properties
        // to describe completely both the hashing and the encryption.
        //public virtual string HashAlgorithm { get; set; }
        //public virtual string SecretSalt { get; set; }

        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? LastLoginUtc { get; set; }
    }
}