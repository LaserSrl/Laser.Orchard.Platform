using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public abstract class BaseMultiStepAccountController : Controller, IMultiStepAccountController {
        // This provides a base implementation for IMultiStepAccountController, and should be inherited
        // by controllers providing final implementations of the interface.
    }
}