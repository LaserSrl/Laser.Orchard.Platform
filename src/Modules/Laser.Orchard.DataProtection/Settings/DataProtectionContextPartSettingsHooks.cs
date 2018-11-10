using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Settings {
    public class DataProtectionContextPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "DataProtectionContextPart") yield break;
            var model = definition.Settings.GetModel<DataProtectionContextPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DataProtectionContextPart") yield break;
            var model = new DataProtectionContextPartSettings();
            updateModel.TryUpdateModel(model, "DataProtectionContextPartSettings", null, null);
            builder.WithSetting("DataProtectionContextPartSettings.ContextDefault", model.ContextDefault);
            builder.WithSetting("DataProtectionContextPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}