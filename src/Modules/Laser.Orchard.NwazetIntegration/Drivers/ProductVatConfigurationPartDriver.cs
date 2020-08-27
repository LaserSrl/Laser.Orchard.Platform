using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    /// <summary>
    /// Insert a shape in products' editor views to allow users to input
    /// the price after tax, and have the taxable price be computed in place
    /// based on the VAT rate (for the default territory).
    /// </summary>
    public class ProductVatConfigurationPartDriver : ContentPartDriver<ProductVatConfigurationPart> {

        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IVatConfigurationProvider _vatConfigurationProvider;

        public ProductVatConfigurationPartDriver(
            IVatConfigurationService vatConfigurationService,
            IVatConfigurationProvider vatConfigurationProvider) {

            _vatConfigurationService = vatConfigurationService;
            _vatConfigurationProvider = vatConfigurationProvider;
        }

        protected override string Prefix {
            get { return "ProductVatConfigurationPart"; }
        }

        protected override DriverResult Editor(ProductVatConfigurationPart part, dynamic shapeHelper) {
            var model = CreateVM(part);
            return ContentShape("Parts_ProductPriceWithVAT_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/ProductPriceWithVATEditor",
                    Model: model,
                    Prefix: Prefix
                    ));
        }

        public ProductPriceEditorViewModel CreateVM(ProductVatConfigurationPart part) {
            var productPart = part.As<ProductPart>();

            var vatRates = _vatConfigurationProvider
                .GetVatConfigurations()
                .Select(vcp => new {
                    id = vcp.Id,
                    rate = _vatConfigurationService.GetRate(vcp)
                })
                .ToDictionary(a => a.id, a => a.rate);
            vatRates.Add(0, _vatConfigurationService.GetRate(_vatConfigurationService.GetDefaultCategory()));

            return new ProductPriceEditorViewModel() {
                BasePrice = productPart != null 
                    ? productPart.Price
                    : 0.0m,
                DiscountPrice = productPart != null
                    ? productPart.DiscountPrice
                    : 0.0m,
                SelectedVatConfigurationId = part.UseDefaultVatCategory
                    ? 0
                    : part.VatConfigurationPart.Record.Id,
                DefaultVatConfigurationId = _vatConfigurationService
                    .GetDefaultCategoryId(),
                DefaultTerritoryName = _vatConfigurationService
                    .GetDefaultDestination()
                    ?.Name ?? string.Empty,
                VatRates = vatRates
            };
        }
    }
}