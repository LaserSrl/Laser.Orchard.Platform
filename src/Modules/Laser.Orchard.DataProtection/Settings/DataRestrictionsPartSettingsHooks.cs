using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.DataProtection.Settings {
    public class DataRestrictionsPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "DataRestrictionsPart") yield break;
            var model = definition.Settings.GetModel<DataRestrictionsPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DataRestrictionsPart") yield break;
            var model = new DataRestrictionsPartSettings();
            updateModel.TryUpdateModel(model, "DataRestrictionsPartSettings", null, null);
            builder.WithSetting("DataRestrictionsPartSettings.RestrictionsDefault", model.RestricitonsDefault);
            builder.WithSetting("DataRestrictionsPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}