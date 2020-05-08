using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class GTMProductService : IGTMProductService {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;
        private readonly IProductPriceService _productPriceService;

        public GTMProductService(
            IOrchardServices orchardServicies,
            ITokenizer tokenizer,
            IProductPriceService productPriceService) {

            _orchardServices = orchardServicies;
            _tokenizer = tokenizer;
            _productPriceService = productPriceService;
        }

        public void FillPart(GTMProductPart part) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (part.ContentItem == null) {
                throw new ArgumentNullException("part.ContentItem");
            }
            var product = part.As<ProductPart>();
            if (product == null) {
                throw new ArgumentNullException("part.A<ProductPart>()");
            }
            var partSetting = part.Settings.GetModel<GTMProductSettingVM>();

            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

            if (partSetting.Id == TypeId.Id) {
                part.ProductId = product.Id.ToString();
            } else {
                part.ProductId = product.Sku;
            }

            part.Name = FillString(partSetting.Name, tokens);
            part.Brand = FillString(partSetting.Brand, tokens);
            part.Category = FillString(partSetting.Category, tokens);
            part.Variant = FillString(partSetting.Variant, tokens);

            // consider discounts
            if (product.DiscountPrice >= 0 && product.DiscountPrice < product.Price) {
                part.Price = _productPriceService.GetDiscountPrice(product);
            } else {
                part.Price = _productPriceService.GetPrice(product);
            }
        }

        public string GetJsonString(GTMProductPart part) {
            if (part == null) {
                return string.Empty;
            }

            FillPart(part);
            return GetJsonString(new GTMProductVM(part));
        }

        public string GetJsonString(GTMProductVM vm) {
            if (vm == null) {
                return string.Empty;
            }
            string output = JsonConvert
                .SerializeObject(vm);

            return output;
        }
        public string GetJsonString(GTMActionField af) {
            if (af == null) {
                return string.Empty;
            }
            string output = JsonConvert
                .SerializeObject(af);

            return output;
        }

        #region private methods to handle tokenized fields
        // anything inserted into these tokens is thrown on the screen
        // even if there is html code
        private string FillString(string value, Dictionary<string, object> tokens) {
            if (!string.IsNullOrEmpty(value)) {
                // if a field like the bodypart with a lot of text is added, 
                // it is better to cut the string in order not to give problems to the page
                // the maximum length of the field is 255 characters, for the moment it's okay
                var maxLenght = 255;
                var str = HttpUtility.HtmlDecode(_tokenizer.Replace(value, tokens));
                if (str.Length <= maxLenght) {
                    return str;
                }
                else {
                    return str.Substring(0, maxLenght);
                }
            }
            return string.Empty;
        }
        #endregion
    }
}