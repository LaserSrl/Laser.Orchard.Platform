using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;


namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.PerItemCache")]
    public class PerItemCachePartSettingsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "PerItemCachePart") yield break;
            var model = definition.Settings.GetModel<PerItemCachePartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "PerItemCachePart") yield break;
            var model = new PerItemCachePartSettings();
            updateModel.TryUpdateModel(model, "PerItemCachePartSettings", null, null);

            // carica ogni campo dei settings
            builder.WithSetting("PerItemCachePartSettings.PerItemKeyParam", model.PerItemKeyParam);

            yield return DefinitionTemplate(model);
        }
    }
}