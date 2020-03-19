using HtmlAgilityPack;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Tokens;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IGTMProductService : IDependency {
        void FillPart(ProductPart product, GTMProductPart part);
    }

    public class GTMProductService : IGTMProductService {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;

        public GTMProductService(
            IOrchardServices orchardServicies,
            ITokenizer tokenizer) {
            _orchardServices = orchardServicies;
            _tokenizer = tokenizer;
        }

        public void FillPart(ProductPart product, GTMProductPart part) {
            var partSetting = part.Settings.GetModel<GTMProductSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
           
            if(partSetting.Id== Enums.TypeId.Id) {
                part.ProductId = product.Id.ToString();
            }
            else {
                part.ProductId = product.Sku;
            }

            part.Name = ProcessString(FillString(partSetting.Name, tokens), true);
            part.Brand = ProcessString(FillString(partSetting.Brand, tokens), true);
            part.Category = ProcessString(FillString(partSetting.Category, tokens), true);
            part.Variant = ProcessString(FillString(partSetting.Variant, tokens), true);

            part.Price = product.ProductPriceService.GetPrice();
            //in questa parte del dettaglio non va popolata
            part.Quantity = 0;
            //per il momento non sono trattati
            part.Coupon = string.Empty;
            part.Position = 0;
        }

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
                }
                else { //just shear it off to avoid breaking stuff db-side
                    value = value.Substring(0, length);
                }
            }
            return value;
        }
    }
}