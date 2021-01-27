using Laser.Orchard.WebServices.Controllers;
using Laser.Orchard.WebServices.Helpers;
using Laser.Orchard.WebServices.Models;
using Laser.Orchard.WebServices.ViewModels;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.WebServices.Drivers {
    [OrchardFeature("Laser.Orchard.CustomRestApi")]
    public class CustomRestApiSiteSettingsPartDriver : ContentPartDriver<CustomRestApiSiteSettingsPart> {

        public CustomRestApiSiteSettingsPartDriver() {

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        protected override string Prefix {
            get {
                return "CustomRestApiSiteSettingsPart";
            }
        }

        protected override DriverResult Editor(CustomRestApiSiteSettingsPart part, dynamic shapeHelper) {
            return ShowEditor(new CustomRestApiSiteSettingsPartViewModel(part), shapeHelper);
        }

        protected override DriverResult Editor(CustomRestApiSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new CustomRestApiSiteSettingsPartViewModel();
            if (updater is CustomRestApiSettingsAdminController
                && updater.TryUpdateModel(model, Prefix, null, null)) {
                var validationErrors = ActionsConfigurationIsValid(model);
                if (validationErrors.Any()) {
                    foreach (var err in validationErrors) {
                        updater.AddModelError("ConfigurationJson", err);
                    }
                    return ShowEditor(model, shapeHelper);
                } else {
                    part.ConfigurationJson = JsonConvert.SerializeObject(model.GetActionsConfiguration(), Formatting.Indented);
                }
            }
            return Editor(part, shapeHelper);
        }

        private DriverResult ShowEditor(CustomRestApiSiteSettingsPartViewModel vm, dynamic shapeHelper) {
            return ContentShape("SiteSettings_CustomRestApiSiteSettingsPart",
                () => shapeHelper.EditorTemplate(
                    Prefix: Prefix,
                    TemplateName: "SiteSettings/CustomRestApiSiteSettingsPart",
                    Model: vm)
                ).OnGroup(CustomRestApiHelper.SettingsGroupId);
        }

        private IEnumerable<LocalizedString> ActionsConfigurationIsValid(
            CustomRestApiSiteSettingsPartViewModel vm) {
            // TODO: this method should be improved to have a validation on the verbs.
            // Also, make sure this method stays up to date whenever the RestApiAction
            // class gets changed.
            IEnumerable<RestApiAction> configs;
            try {
                configs = vm.GetActionsConfiguration();
            } catch (Exception) {
                configs = null;
            }
            if (configs == null) {
                yield return T("Failed to parse the json.");
            } else {
                // custom validation on each object in the array
                foreach (var item in configs) {
                    if (string.IsNullOrWhiteSpace(item.Name)) {
                        yield return T("Action Name is required.");
                    }
                }
                var groups = configs.GroupBy(raa => raa.Name);
                foreach (var dup in groups.Where(g => g.Count() > 1)) {
                    yield return T("Action Name is duplicate: {0}", dup.Key);
                }
            }
        }
    }
}