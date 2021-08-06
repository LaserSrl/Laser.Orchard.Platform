using Laser.Orchard.NwazetIntegration.Models;
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
    public class ProductVatConfigurationPartInputPriceEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "ProductVatConfigurationPart") {
                var model = definition.Settings
                    .GetModel<ProductVatConfigurationPartInputPriceSettings>();

                var vm = new ProductVatConfigurationPartInputPriceSettingsVM(model);

                yield return DefinitionTemplate(vm);
            }
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {

            if (builder.Name == "ProductVatConfigurationPart") {
                var vm = new ProductVatConfigurationPartInputPriceSettingsVM();

                if (updateModel.TryUpdateModel(vm, "ProductVatConfigurationPartInputPriceSettingsVM", null, null)) {
                    builder.WithSetting("ProductVatConfigurationPartInputPriceSettings.InputFinalPrice",
                        vm.InputFinalPrice.ToString());
                }

                yield return DefinitionTemplate(vm);
            }
        }
    }
}