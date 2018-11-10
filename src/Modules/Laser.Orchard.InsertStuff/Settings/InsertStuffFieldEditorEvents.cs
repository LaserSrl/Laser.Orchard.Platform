using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.InsertStuff.Settings {

    public class InsertStuffEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "InsertStuffField") {
                var model = definition.Settings.GetModel<InsertStuffFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "InsertStuffField") {
                yield break;
            }

            var model = new InsertStuffFieldSettings();
            if (updateModel.TryUpdateModel(model, "InsertStuffFieldSettings", null, null)) {
                builder.WithSetting("InsertStuffFieldSettings.StyleList", model.StyleList);
                builder.WithSetting("InsertStuffFieldSettings.ScriptList", model.ScriptList);
                builder.WithSetting("InsertStuffFieldSettings.RawHtml", model.RawHtml);
                builder.WithSetting("InsertStuffFieldSettings.OnFooter", (model.OnFooter ? "true" : "false"));
            }

            yield return DefinitionTemplate(model);
        }
    }
}