using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Settings {
    public class PushMobileEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "MobilePushPart") yield break;
            var model = definition.Settings.GetModel<PushMobilePartSettingVM>();

            //model.AvailableModes = Enum.GetValues(typeof(ShareBarMode))
            //    .Cast<int>()
            //    .Select(i =>
            //        new {
            //            Text = Enum.GetName(typeof(ShareBarMode), i),
            //            Value = i
            //        });

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

                if (builder.Name != "MobilePushPart") yield break;

                var model = new PushMobilePartSettingVM();
                updateModel.TryUpdateModel(model, "PushMobilePartSettingVM", null, null);
                builder.WithSetting("PushMobilePartSettingVM.HideRelated",
                ((Boolean)model.HideRelated).ToString());
                builder.WithSetting("PushMobilePartSettingVM.AcceptZeroRelated",
                ((Boolean)model.AcceptZeroRelated).ToString());
                builder.WithSetting("PushMobilePartSettingVM.QueryDevice",
             ((string)model.QueryDevice) ?? "");
            yield return DefinitionTemplate(model);
        }

    }
}