using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.StartupConfig.Settings {
    public class ContentPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentPickerField") {
                var model = definition.Settings.GetModel<ContentPickerFieldExtensionSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var model = new ContentPickerFieldExtensionSettings();
            if (updateModel.TryUpdateModel(model, "ContentPickerFieldExtensionSettings", null, null)) {
                builder.WithSetting("ContentPickerFieldExtensionSettings.CascadePublish", model.CascadePublish.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldExtensionSettings.SummaryView", model.SummaryView);
                builder.WithSetting("ContentPickerFieldExtensionSettings.PreviewSelected", model.PreviewSelected); 
            }
            yield return DefinitionTemplate(model);
        }
    }
}