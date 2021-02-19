using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsPart : ContentPart<ApiCredentialsPartRecord> {
        public string ApiKey {
            get { return Retrieve(x => x.ApiKey); }
            set { Store(x => x.ApiKey, value); }
        }

        public string ApiSecret {
            get { return Retrieve(x => x.ApiSecret); }
            set { Store(x => x.ApiSecret, value); }
        }

        public string ApiSecretHash {
            get { return Retrieve(x => x.ApiSecretHash); }
            set { Store(x => x.ApiSecretHash, value); }
        }
        public string HashAlgorithm {
            get { return Retrieve(x => x.HashAlgorithm); }
            set { Store(x => x.HashAlgorithm, value); }
        }
        public string SecretSalt {
            get { return Retrieve(x => x.SecretSalt); }
            set { Store(x => x.SecretSalt, value); }
        }

        public DateTime? CreatedUtc {
            get { return Retrieve(x => x.CreatedUtc); }
            set { Store(x => x.CreatedUtc, value); }
        }

        // TODO: do we need this? We are not using it currently. May be we should
        // update the UserPart's LastLoginUtc when logging in with Api credentials?
        public DateTime? LastLoginUtc {
            get { return Retrieve(x => x.LastLoginUtc); }
            set { Store(x => x.LastLoginUtc, value); }
        }
    }
}