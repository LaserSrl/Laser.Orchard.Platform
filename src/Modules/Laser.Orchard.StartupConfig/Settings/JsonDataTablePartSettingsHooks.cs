using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.JsonDataTablePart")]
    public class JsonDataTablePartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "JsonDataTablePart") yield break;
            var model = definition.Settings.GetModel<JsonDataTablePartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "JsonDataTablePart") yield break;
            var model = new JsonDataTablePartSettings();
            updateModel.TryUpdateModel(model, "JsonDataTablePartSettings", null, null);
            // carica ogni campo dei settings
            builder.WithSetting("JsonDataTablePartSettings.ColumnsDefinition", model.ColumnsDefinition);
            builder.WithSetting("JsonDataTablePartSettings.MaxRows", Convert.ToString(model.MaxRows));
            yield return DefinitionTemplate(model);
        }
    }
}