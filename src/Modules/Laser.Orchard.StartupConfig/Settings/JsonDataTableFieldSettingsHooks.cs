using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class JsonDataTableFieldSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "JsonDataTableField") {
                var model = definition.Settings.GetModel<JsonDataTableFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "JsonDataTableField") {
                yield break;
            }
            var model = new JsonDataTableFieldSettings();
            if (updateModel.TryUpdateModel(model, "JsonDataTableFieldSettings", null, null)) {
                builder.WithSetting("JsonDataTableFieldSettings.ColumnsDefinition", model.ColumnsDefinition);
                builder.WithSetting("JsonDataTableFieldSettings.MaxRows", model.MaxRows.ToString());
            }
            yield return DefinitionTemplate(model);
        }
    }
}