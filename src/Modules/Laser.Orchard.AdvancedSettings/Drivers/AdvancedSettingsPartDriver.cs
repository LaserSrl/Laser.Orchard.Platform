using Laser.Orchard.AdvancedSettings.Models;
using Laser.Orchard.AdvancedSettings.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdvancedSettings.Drivers {
    public class AdvancedSettingsPartDriver : ContentPartDriver<AdvancedSettingsPart> {
        private readonly IContentManager _contentManager;

        public AdvancedSettingsPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "AdvancedSettingsPart"; }
        }

        protected override DriverResult Editor(AdvancedSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AdvancedSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new AdvancedSettingsEdit();
            model.Name = part.Name;
            model.EditableName = string.IsNullOrWhiteSpace(part.Name);
            if (updater != null) {
                if (updater.TryUpdateModel(model, Prefix, null, null)) {
                    var settings = _contentManager.Query<AdvancedSettingsPart, AdvancedSettingsPartRecord>().Where(x => x.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) && x.Id != part.Id).Count();
                    if (settings > 0) {
                        updater.AddModelError("Name", T("A setting with the same Name already exists."));
                        model.EditableName = true;
                    }
                    else {
                        part.Name = model.Name;
                    }
                }
            }

            return ContentShape("Parts_AdvancedSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/AdvancedSettings.Edit", Model: model, Prefix: Prefix));
        }

        protected override void Importing(AdvancedSettingsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }
            part.Name = context.Attribute(part.PartDefinition.Name, "Name");

            // TODO: Manage multiple theme settings
        }

        protected override void Exporting(AdvancedSettingsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Name", part.Name);

            // TODO: Manage multiple theme settings
        }
    }
}