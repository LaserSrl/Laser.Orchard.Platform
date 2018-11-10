using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Services;
using Laser.Orchard.ButtonToWorkflows.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.ButtonToWorkflows.Drivers {
    public class DynamicButtonToWorkflowsDriver : ContentPartDriver<DynamicButtonToWorkflowsPart> {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        private readonly IOrchardServices _orchardServices;

        public DynamicButtonToWorkflowsDriver(IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService, IOrchardServices orchardServices) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            _orchardServices = orchardServices;
        }

        protected override string Prefix
        {
            get { return "Laser.Orchard.DynamicButtonToWorkflows"; }
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsPart part, dynamic shapeHelper) {

            var settings = part.TypePartDefinition.Settings.GetModel<DynamicButtonsSetting>();
            var buttonList = _dynamicButtonToWorkflowsService.GetButtons().Where(w => settings.List.ToList().Contains("{" + w.GlobalIdentifier + "}"));

            return ContentShape("Parts_DynamicButtonToWorkflows", () => shapeHelper.EditorTemplate(TemplateName: "Parts/DynamicButtonToWorkflows",
                                                                                                   Model: buttonList.ToList(),
                                                                                                   Prefix: Prefix));
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsPart part, IUpdateModel updater, dynamic shapeHelper) {
            List<DynamicButtonToWorkflowsRecord> model = new List<DynamicButtonToWorkflowsRecord>();

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                if (part.ContentItem.Id != 0) {
                    foreach (DynamicButtonToWorkflowsRecord button in model) {
                        if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.DynamicCustomButton." + button.GlobalIdentifier) {
                            part.ButtonName = button.ButtonName;
                            part.MessageToWrite = button.ButtonMessage;
                            part.ActionAsync = button.ButtonAsync;
                        }
                    }
                }
            }

            return Editor(part, shapeHelper);
        }
    }
}