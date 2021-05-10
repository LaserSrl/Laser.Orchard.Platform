using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Settings {
    public class HashedStringFieldEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "HashedStringField") {
                var model = definition.Settings.GetModel<HashedStringFieldSettings>();
                // Yield is used to return a single element in a IEnumerable.
                // It executes the code for every element in the IEnumerable.
                // If there's some code after the "yield return" statement, that code is executed (the return doesn't exit from the function).
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "HashedStringField") {
                yield break;
            }

            var model = new HashedStringFieldSettings();
            if (updateModel.TryUpdateModel(model, "HashedStringFieldSettings", null, null)) {
                builder.WithSetting("HashedStringFieldSettings.Hint", model.Hint);
                builder.WithSetting("HashedStringFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("HashedStringFieldSettings.Pattern", model.Pattern);
                builder.WithSetting("HashedStringFieldSettings.ConfirmRequired", model.ConfirmRequired.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}