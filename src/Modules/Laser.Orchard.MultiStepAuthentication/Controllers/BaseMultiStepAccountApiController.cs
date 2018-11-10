using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public class BaseMultiStepAccountApiController : ApiController, IMultiStepAccountApiController {
        // This provides a base implementation for IMultiStepAccountApiController, and should be inherited
        // by controllers providing final implementations of the interface.
    }
}