using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsPartRecord : ContentPartRecord {

        public virtual string ApiKey { get; set; }
        // This is an encrypted form of the secret, so we are able to retrieve it.
        // If we upgrade/change the way we encrypt this, we should be careful to 
        // upgrade existing values in the db accordinlgy, or support some way to
        // identify that we are encrypting them with an old technique. Generally, 
        // we should probably go for the first (upgrade values), because the likely
        // cause to update the encryption technique is that a security issue was found.
        public virtual string ApiSecret { get; set; }

        // when we authenticate using these credentials.
        // Hence, here we'll need a bunch of additional fields/properties
        // to describe completely the hashing.
        public virtual string ApiSecretHash { get; set; }
        public virtual string HashAlgorithm { get; set; }
        public virtual string SecretSalt { get; set; }

        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? LastLoginUtc { get; set; }
    }
}