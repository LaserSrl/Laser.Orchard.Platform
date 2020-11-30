using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Settings {
    public class CacheEvictorPartSettingsHook : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CacheEvictorPart") yield break;
            var model = definition.Settings.GetModel<CacheEvictorPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CacheEvictorPart") yield break;
            var model = new CacheEvictorPartSettings();
            updateModel.TryUpdateModel(model, "CacheEvictorPartSettings", null, null);

            // carica ogni campo dei settings
            builder.WithSetting("CacheEvictorPartSettings.EvictItem", model.EvictItem);

            yield return DefinitionTemplate(model);
        }
    }
}