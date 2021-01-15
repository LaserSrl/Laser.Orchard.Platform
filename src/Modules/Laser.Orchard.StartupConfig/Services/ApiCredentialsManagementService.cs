using Laser.Orchard.StartupConfig.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsManagementService : IApiCredentialsManagementService {

        public string GetSecret(ApiCredentialsPart part) {
            // TODO: decryption
            return part.Secret;
        }
    }
}