using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class ProductPriceEditorViewModel {

        public ProductPriceEditorViewModel() {
            VatRates = new Dictionary<int, decimal>();
            VATDropDownId = "ProductVatConfigurationPart_VatConfigurationId";
            OriginalPriceId = "NwazetCommerceProduct_Product_Price";
            DiscountPriceId = "NwazetCommerceProduct_DiscountPrice";

            ShowShape = false;
        }

        public ProductPriceEditorViewModel(
            ProductVatConfigurationPart vatPart)
            : this() { }

        public ProductPriceEditorViewModel(
            ProductVatConfigurationPart vatPart,
            ProductPart product,
            IProductPriceService productPriceService) 
            : this(vatPart) {

            ShowShape = true;
            HasDiscount = true;
            BasePrice = product != null
                    ? product.Price
                    : 0.0m;
            DiscountPrice = product != null
                ? product.DiscountPrice
                : -1.0m;
            BaseTaxedPrice = product != null && productPriceService != null
                ? productPriceService.GetPrice(product)
                : 0.0m;
            DiscountTaxedPrice = product != null && productPriceService != null
                ? productPriceService.GetDiscountPrice(product) >= 0
                    ? productPriceService.GetDiscountPrice(product) : -1.0m
                : -1.0m;
        }

        public ProductPriceEditorViewModel(
            ProductVatConfigurationPart vatPart,
            IShippingMethod shipping,
            IVatConfigurationService vatConfigurationService)
            : this(vatPart) {

            // There are two different cases here: FlexibleShippingMethodPart or the others
            HasDiscount = false;
            var rate = vatConfigurationService.GetRate(vatPart.VatConfigurationPart ?? vatConfigurationService.GetDefaultCategory());
            if (shipping is FlexibleShippingMethodPart) {
                ShowShape = true;
                var flexible = shipping as FlexibleShippingMethodPart;
                BasePrice = flexible.DefaultPrice;
                BaseTaxedPrice = flexible.DefaultPrice * (1.0m + rate);
                OriginalPriceId = "FlexibleShippingMethodPart_DefaultPrice";
            }
            // NOTE: VAT configuration currently only works properly and is tested
            // for FlexibleShippingMethodPart. Other IShippingMethod implementations
            // likely don't work with it.
        }

        public decimal BasePrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal BaseTaxedPrice { get; set; }
        public decimal DiscountTaxedPrice { get; set; }
        // should DiscountPrice be used/displayed/edited?
        public bool HasDiscount { get; set; }
        // should we show anything?
        public bool ShowShape { get; set; }

        public int SelectedVatConfigurationId { get; set; }
        public int DefaultVatConfigurationId { get; set; }

        public string DefaultTerritoryName { get; set; }

        public Dictionary<int, decimal> VatRates { get; set; }

        // strings used to "find" stuff in the page
        public string VATDropDownId { get; set; }
        public string OriginalPriceId { get; set; }
        public string DiscountPriceId { get; set; }

    }
}