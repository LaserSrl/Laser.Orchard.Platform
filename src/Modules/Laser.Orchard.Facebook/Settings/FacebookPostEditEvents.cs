using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.Facebook.Settings {

    public class FacebookPostEditEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "FacebookPostPart") yield break;
            var model = definition.Settings.GetModel<FacebookPostPartSettingVM>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "FacebookPostPart") yield break;
            var model = new FacebookPostPartSettingVM();
            updateModel.TryUpdateModel(model, "FacebookPostPartSettingVM", null, null);
            builder.WithSetting("FacebookPostPartSettingVM.FacebookCaption", ((string)model.FacebookCaption) ?? "");
            builder.WithSetting("FacebookPostPartSettingVM.FacebookMessage", ((string)model.FacebookMessage) ?? "");
            builder.WithSetting("FacebookPostPartSettingVM.FacebookDescription", ((string)model.FacebookDescription) ?? "");
            builder.WithSetting("FacebookPostPartSettingVM.FacebookName", ((string)model.FacebookName) ?? "");
            builder.WithSetting("FacebookPostPartSettingVM.FacebookPicture", ((string)model.FacebookPicture) ?? "");
            builder.WithSetting("FacebookPostPartSettingVM.FacebookLink", ((string)model.FacebookLink) ?? "");
            yield return DefinitionTemplate(model);
        }
    }
}