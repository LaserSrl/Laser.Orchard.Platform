using HtmlAgilityPack;
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

            part.Name = ProcessString(FillString(partSetting.Name, tokens), true);
            part.Brand = ProcessString(FillString(partSetting.Brand, tokens), true);
            part.Category = ProcessString(FillString(partSetting.Category, tokens), true);
            part.Variant = ProcessString(FillString(partSetting.Variant, tokens), true);

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
        private string FillString(string value, Dictionary<string, object> tokens) {
            if (!string.IsNullOrEmpty(value)) {
                return _tokenizer.Replace(value, tokens);
            }
            return string.Empty;
        }
        private string ProcessString(string value, bool removeHtml) {
            return ProcessString(value, 0, removeHtml);
        }
        private string ProcessString(string value, int length = 0, bool removeHtml = false) {
            if (removeHtml) {
                value = HttpUtility.HtmlDecode(value);
                value = RemoveHtmlTag(value);
            }
            return TruncateText(value, length);
        }

        private string RemoveHtmlTag(string text) {
            if (string.IsNullOrEmpty(text))
                return "";
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);
            return (htmlDoc.DocumentNode.InnerText);
        }
        private string TruncateText(string value, int length = 0) {
            if (string.IsNullOrWhiteSpace(value) || value.Length < length || length <= 0) {
                return value;
            }

            value = value.Substring(0, value.IndexOf(" ", length));
            if (value.Length > length) {
                if (value.LastIndexOf(" ") != -1) { //try to be nice and truncate aftrer a word
                    value = value.Substring(0, value.LastIndexOf(" "));
                } else { //just shear it off to avoid breaking stuff db-side
                    value = value.Substring(0, length);
                }
            }
            return value;
        }
        #endregion
    }
}