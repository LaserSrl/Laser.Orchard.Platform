using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;

namespace Laser.Orchard.NwazetIntegration.PartSettings {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "FacebookShopProductPart") yield break;
            var model = definition.Settings.GetModel<FacebookShopProductPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {
            if (builder.Name != "FacebookShopProductPart") yield break;
            var model = new FacebookShopProductPartSettings();
            updateModel.TryUpdateModel(model, "FacebookShopProductPartSettings", null, null);
            builder.WithSetting("FacebookShopProductPartSettings.JsonForProductUpdate", ((string)model.JsonForProductUpdate) ?? "");
            yield return DefinitionTemplate(model);
        }
    }
}