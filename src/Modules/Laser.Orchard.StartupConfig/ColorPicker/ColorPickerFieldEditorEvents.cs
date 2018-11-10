using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ColorPicker {
    public class ColorPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ColorPickerField") {
                var model = definition.Settings.GetModel<ColorPickerFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ColorPickerFieldSettings();
            if (builder.FieldType != "ColorPickerField") {
                yield break;
            }
            if (updateModel.TryUpdateModel(model, "ColorPickerFieldSettings", null, null)) {
                builder.WithSetting("ColorPickerFieldSettings.Hint", model.Hint);
                builder.WithSetting("ColorPickerFieldSettings.Options", model.Options);
            }
            yield return DefinitionTemplate(model);
        }
    }
}