using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Settings {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class TemplatePartSettingsEvents : ContentDefinitionEditorEventsBase {
        
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "TemplatePart")
                yield break;

            var model = definition.Settings.GetModel<TemplatePartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "TemplatePart")
                yield break;

            var model = new TemplatePartSettings();

            if (updateModel.TryUpdateModel(model, "TemplatePartSettings", null, null)) {
                builder.WithSetting("TemplatePartSettings.DefaultParserIdSelected", model.DefaultParserIdSelected);

                yield return DefinitionTemplate(model);
            }
        }
    }
}