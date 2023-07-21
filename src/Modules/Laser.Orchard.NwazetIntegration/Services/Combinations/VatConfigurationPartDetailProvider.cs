using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Services.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services.Combinations {
    public class VatConfigurationPartDetailProvider :
        BaseCombinationDetailProvider {
        public VatConfigurationPartDetailProvider(
           IContentDefinitionManager contentDefinitionManager)
           : base(contentDefinitionManager) {
        }

        public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
           ContentTypeDefinitionBuilder previous,
           string containerTypeName) {

            var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

            if (containerDefinition != null
                && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProductVatConfigurationPart")) {
                // alter the definition
                var vm = containerDefinition
                    .Parts
                    .FirstOrDefault(ctpd => ctpd.PartDefinition.Name == "ProductVatConfigurationPart")
                    .Settings
                    .GetModel<ProductVatConfigurationPartInputPriceSettings>();
                return previous
                    .WithPart("ProductVatConfigurationPart", pb => pb.WithSetting("ProductVatConfigurationPartInputPriceSettings.InputFinalPrice", vm.InputFinalPrice.ToString()));
            }
            return previous;
        }
    }
}