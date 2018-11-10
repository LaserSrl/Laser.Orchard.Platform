using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Services;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Laser.Orchard.ButtonToWorkflows.Drivers {

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsSettingsPartDriver : ContentPartDriver<DynamicButtonToWorkflowsSettingsPart> {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        private const string TemplateName = "Parts/DynamicButtonToWorkflowsSettings";

        public Localizer T { get; set; }
        public ILogger _logger { get; set; }

        public DynamicButtonToWorkflowsSettingsPartDriver(
            IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService,
            IControllerContextAccessor controllerContextAccessor) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            _logger = NullLogger.Instance;
            _controllerContextAccessor = controllerContextAccessor;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get { return "Laser.DynamicButtonToWorkflows.Settings"; }
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_DynamicButtonToWorkflowsSettings_Edit",
                () => {
                    IEnumerable<DynamicButtonToWorkflowsEdit> buttons = null;
                    var buttonsWithErrors = _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"];
                    if (buttonsWithErrors == null)
                        buttons = _dynamicButtonToWorkflowsService.GetButtons().Select(s => new DynamicButtonToWorkflowsEdit {
                            Id = s.Id,
                            ButtonName = s.ButtonName,
                            ButtonText = s.ButtonText,
                            ButtonDescription = s.ButtonDescription,
                            ButtonMessage = s.ButtonMessage,
                            ButtonAsync = s.ButtonAsync,
                            GlobalIdentifier = s.GlobalIdentifier,
                            Delete = false
                        });
                    else
                        buttons = ((IEnumerable<DynamicButtonToWorkflowsEdit>)buttonsWithErrors).Where(x => x.Delete == false);

                    var model = new DynamicButtonToWorkflowsSettingsVM {
                        Buttons = buttons
                    };

                    return shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: model,
                    Prefix: Prefix);
                }).OnGroup("Buttons");
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            DynamicButtonToWorkflowsSettingsVM dynamicButtonSettingsVM = new DynamicButtonToWorkflowsSettingsVM();

            if (updater.TryUpdateModel(dynamicButtonSettingsVM, Prefix, null, null)) {
                var buttonsByName = dynamicButtonSettingsVM.Buttons.Where(w => w.Delete == false).GroupBy(q => new { q.ButtonName }).Select(q => new { q.Key.ButtonName, Occurrences = q.Count() });
                buttonsByName = buttonsByName.Where(q => q.Occurrences > 1);

                if (buttonsByName.ToList().Count() > 0) {
                    _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"] = dynamicButtonSettingsVM.Buttons;
                    updater.AddModelError("ButtonUpdateError", T("Cannot have multiple buttons with the same name ({0})", string.Join(", ", buttonsByName.Select(q => q.ButtonName))));
                } else {
                    _dynamicButtonToWorkflowsService.UpdateButtons(dynamicButtonSettingsVM.Buttons);
                }
            } else {
                _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"] = dynamicButtonSettingsVM.Buttons;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(DynamicButtonToWorkflowsSettingsPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);

            foreach (var button in _dynamicButtonToWorkflowsService.GetButtons().OrderBy(x => x.Id)) {
                XElement buttonSettings = new XElement("ButtonSettings");
                buttonSettings.SetAttributeValue("ButtonName", button.ButtonName);
                buttonSettings.SetAttributeValue("ButtonText", button.ButtonText);
                buttonSettings.SetAttributeValue("ButtonDescription", button.ButtonDescription);
                buttonSettings.SetAttributeValue("ButtonMessage", button.ButtonMessage);
                buttonSettings.SetAttributeValue("ButtonAsync", button.ButtonAsync);
                buttonSettings.SetAttributeValue("GlobalIdentifier", button.GlobalIdentifier);
                root.Add(buttonSettings);
            }
        }

        protected override void Importing(DynamicButtonToWorkflowsSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            if (root == null) {
                return;
            }
            var newDefinitions = new List<DynamicButtonToWorkflowsEdit>();
            var existingDefinitions = _dynamicButtonToWorkflowsService.GetButtons();
            var buttonSettings = root.Elements("ButtonSettings");

            foreach (var button in buttonSettings) {
                var uniqueName = "";
                if (button.Attribute("ButtonName") != null && !string.IsNullOrWhiteSpace(button.Attribute("ButtonName").Value)) {
                    uniqueName = GenerateUniqueName(button.Attribute("ButtonName").Value, existingDefinitions.ToList());

                    if (existingDefinitions.Any(a => a.ButtonName.Trim().Equals(button.Attribute("ButtonName").Value.Trim())))
                        _logger.Error(string.Format("The button name {0} was already used. It was automatically renamed to {1}.", button.Attribute("ButtonName").Value, uniqueName));
                } else
                    uniqueName = GenerateUniqueName(Guid.NewGuid().ToString(), existingDefinitions.ToList());

                newDefinitions.Add(new DynamicButtonToWorkflowsEdit {
                    ButtonName = uniqueName,
                    ButtonText = button.Attribute("ButtonText") != null ? button.Attribute("ButtonText").Value : "",
                    ButtonDescription = button.Attribute("ButtonDescription") != null ? button.Attribute("ButtonDescription").Value : "",
                    ButtonMessage = button.Attribute("ButtonMessage") != null ? button.Attribute("ButtonMessage").Value : "",
                    ButtonAsync = button.Attribute("ButtonAsync") != null ? bool.Parse(button.Attribute("ButtonAsync").Value) : false,
                    GlobalIdentifier = button.Attribute("GlobalIdentifier") != null ? button.Attribute("GlobalIdentifier").Value : "",
                    Delete = false
                });
            }

            _dynamicButtonToWorkflowsService.UpdateButtons(newDefinitions);
        }

        private string GenerateUniqueName(string name, List<DynamicButtonToWorkflowsRecord> buttons) {
            if (!buttons.Any(a => a.ButtonName.Trim().Equals(name.Trim())))
                return name;
            else {
                int suffix = 2;
                string newName = name + suffix;
                while (buttons.Any(a => a.ButtonName.Trim().Equals(newName.Trim()))) {
                    suffix++;
                    newName = name + suffix;
                }

                return newName;
            }
        }
    }
}