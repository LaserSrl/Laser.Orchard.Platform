using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.SecureData.Settings {
    public class EncryptedStringFieldEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "EncryptedStringField") {
                var model = definition.Settings.GetModel<EncryptedStringFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "EncryptedStringField") {
                yield break;
            }

            var model = new EncryptedStringFieldSettings();
            if (updateModel.TryUpdateModel(model, "EncryptedStringFieldSettings", null, null)) {
                builder.WithSetting("EncryptedStringFieldSettings.Hint", model.Hint);
                builder.WithSetting("EncryptedStringFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("EncryptedStringFieldSettings.Pattern", model.Pattern);
                builder.WithSetting("EncryptedStringFieldSettings.ConfirmRequired", model.ConfirmRequired.ToString());
                builder.WithSetting("EncryptedStringFieldSettings.IsVisible", model.IsVisible.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}