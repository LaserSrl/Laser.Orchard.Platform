using Laser.Orchard.StartupConfig.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.StartupConfig.Controllers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    [RoutePrefix("BearerToken")]
    public class BearerTokenController : ApiController {

        public BearerTokenController() { }

        [HttpPost]
        [AlwaysAccessible]
        [ActionName("Auth")]
        public BearerTokenCreatedRespose Auth(BearerTokenAuthRequest credentials) {
            // todo test credentials
            return new BearerTokenCreatedRespose {
                Token = "pippo"
            };
        }
    }
}