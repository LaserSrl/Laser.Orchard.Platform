using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Drivers {
    public class AuthenticationRequiredPartDriver : ContentPartDriver<AuthenticationRequiredPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public AuthenticationRequiredPartDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
        }
        protected override DriverResult Display(AuthenticationRequiredPart part, string displayType, dynamic shapeHelper) {
            // check sulle permission (esclude il modulo Generator)
            if (_controllerContextAccessor.Context.Controller.GetType().Namespace != "Laser.Orchard.Generator.Controllers") {
                if (_orchardServices.WorkContext.CurrentUser == null) {
                    throw new System.Security.SecurityException("You do not have permission to access this content.");
                }
            }
            return null;
        }
    }
}