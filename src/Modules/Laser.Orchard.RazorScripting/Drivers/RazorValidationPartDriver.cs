using Laser.Orchard.RazorScripting.Models;
using Laser.Orchard.RazorScripting.Settings;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;

namespace Laser.Orchard.RazorScripting.Drivers {

    public class RazorValidationPartDriver : ContentPartDriver<RazorValidationPart> {
        private readonly IRazorExecuteService _razorExecuteService;
        private readonly IOrchardServices _orchardServices;

        private readonly IWorkContextAccessor _workContextAccessor;

        public RazorValidationPartDriver(
            IRazorExecuteService razorExecuteServices,
            IOrchardServices orchardServices,
            IWorkContextAccessor workContextAccessor) {
            _razorExecuteService = razorExecuteServices;
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "RazorValidation"; }
        }

        protected override DriverResult Editor(RazorValidationPart part, IUpdateModel updater, dynamic shapeHelper) {
            var script = part.Settings.GetModel<RazorValidationPartSettings>().Script;
            if (!String.IsNullOrWhiteSpace(script)) {
                var ris = _razorExecuteService.ExecuteString(script, part.ContentItem, null);
                if (!string.IsNullOrEmpty(ris))
                    updater.AddModelError(Prefix, T(ris));
            }
            return Editor(part, shapeHelper);
        }
    }
}