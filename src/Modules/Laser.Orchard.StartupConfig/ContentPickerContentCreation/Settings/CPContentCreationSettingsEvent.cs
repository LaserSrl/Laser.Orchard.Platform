using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]
    public class CPContentCreationSettingsEvent : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentPickerField") {
                var model = definition.Settings.GetModel<CPContentCreationSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var model = new CPContentCreationSettings();
            if (updateModel.TryUpdateModel(model, "CPContentCreationSettings", null, null)) {
                builder.WithSetting("CPContentCreationSettings.EnableContentCreation", model.EnableContentCreation.ToString(CultureInfo.InvariantCulture));
            }
            yield return DefinitionTemplate(model);
        }
    }
}