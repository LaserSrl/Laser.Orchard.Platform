using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Settings {
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