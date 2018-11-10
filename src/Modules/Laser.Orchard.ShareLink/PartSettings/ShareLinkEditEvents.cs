using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.ShareLink.PartSettings {

    public class ShareLinkEditEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ShareLinkPart") yield break;
            var model = definition.Settings.GetModel<ShareLinkPartSettingVM>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "ShareLinkPart") yield break;
            var model = new ShareLinkPartSettingVM();
            updateModel.TryUpdateModel(model, "ShareLinkPartSettingVM", null, null);
            builder.WithSetting("ShareLinkPartSettingVM.SharedLink", ((string)model.SharedLink) ?? "");
            builder.WithSetting("ShareLinkPartSettingVM.SharedBody", ((string)model.SharedBody) ?? "");
            builder.WithSetting("ShareLinkPartSettingVM.SharedText", ((string)model.SharedText) ?? "");
            builder.WithSetting("ShareLinkPartSettingVM.SharedImage", ((string)model.SharedImage) ?? "");
            builder.WithSetting("ShareLinkPartSettingVM.ShowImageChoise", model.ShowImageChoise.ToString());
            builder.WithSetting("ShareLinkPartSettingVM.ShowBodyChoise", model.ShowBodyChoise.ToString());
            builder.WithSetting("ShareLinkPartSettingVM.ShowTextChoise", model.ShowTextChoise.ToString());
            builder.WithSetting("ShareLinkPartSettingVM.ShowLinkChoise", model.ShowLinkChoise.ToString());
            yield return DefinitionTemplate(model);
        }
    }
}