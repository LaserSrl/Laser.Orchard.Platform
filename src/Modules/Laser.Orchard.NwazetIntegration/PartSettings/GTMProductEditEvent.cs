using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.PartSettings {
    public class GTMProductEditEvent : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "GTMProductPart") yield break;
            var model = definition.Settings.GetModel<GTMProductSettingVM>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "GTMProductPart") yield break;
            var model = new GTMProductSettingVM();
            updateModel.TryUpdateModel(model, "GTMProductSettingVM", null, null);
            builder.WithSetting("GTMProductSettingVM.Id", model.Id.ToString());
            builder.WithSetting("GTMProductSettingVM.Name", ((string)model.Name) ?? "");
            builder.WithSetting("GTMProductSettingVM.Brand", ((string)model.Brand) ?? "");
            builder.WithSetting("GTMProductSettingVM.Category", ((string)model.Category) ?? "");
            builder.WithSetting("GTMProductSettingVM.Variant", ((string)model.Variant) ?? "");
            yield return DefinitionTemplate(model);
        }
    }
}