using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Twitter.Settings {

    public class TwitterPostEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "TwitterPostPart") yield break;
            var model = definition.Settings.GetModel<TwitterPostPartSettingVM>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "TwitterPostPart") yield break;
            var model = new TwitterPostPartSettingVM();
            updateModel.TryUpdateModel(model, "TwitterPostPartSettingVM", null, null);
            builder.WithSetting("TwitterPostPartSettingVM.Title", ((string)model.Title) ?? "");
            builder.WithSetting("TwitterPostPartSettingVM.Image", ((string)model.Image) ?? "");
            builder.WithSetting("TwitterPostPartSettingVM.Description", ((string)model.Description) ?? "");
            builder.WithSetting("TwitterPostPartSettingVM.ShowTwitterCurrentLink", ((Boolean)model.ShowTwitterCurrentLink).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}