using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Settings {
    public class DisplayTextPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "DisplayTextPart") yield break;
            var model = definition.Settings.GetModel<DisplayTextPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DisplayTextPart") yield break;
            var model = new DisplayTextPartSettings();
            updateModel.TryUpdateModel(model, "DisplayTextPartSettings", null, null);

            // carica ogni campo dei settings
            builder.WithSetting("DisplayTextPartSettings.DisplayText", model.DisplayText);

            yield return DefinitionTemplate(model);
        }
    }
}