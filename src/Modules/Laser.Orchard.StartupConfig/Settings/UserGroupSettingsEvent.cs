using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UserGroupSettingsEvent : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "UsersGroupsPart") {
                var model = definition.Settings.GetModel<UserGroupSettings>();
                yield return DefinitionTemplate(model);
            }
        }
      
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
                if (builder.Name != "UsersGroupsPart") yield break;
            var model = new UserGroupSettings();
            if (updateModel.TryUpdateModel(model, "UserGroupSettings", null, null)) {
                builder.WithSetting("UserGroupSettings.Required", model.Required.ToString());
            }
            yield return DefinitionTemplate(model);
        }
    }
}

