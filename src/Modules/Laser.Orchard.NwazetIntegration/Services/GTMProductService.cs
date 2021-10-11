using Laser.Orchard.Cookies;
using Laser.Orchard.Cookies.Services;
using Laser.Orchard.GoogleAnalytics.Models;
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
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class GTMProductService : IGTMProductService {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;
        private readonly IProductPriceService _productPriceService;
        private readonly IGDPRScript _gdprScriptService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private GoogleAnalyticsSettingsPart _gaSettings;

        public GTMProductService(
            IOrchardServices orchardServicies,
            ITokenizer tokenizer,
            IProductPriceService productPriceService,
            IGDPRScript gdprScriptService,
            IWorkContextAccessor workContextAccessor) {

            _orchardServices = orchardServicies;
            _tokenizer = tokenizer;
            _productPriceService = productPriceService;
            _gdprScriptService = gdprScriptService;
            _workContextAccessor = workContextAccessor;
        }

        private IEnumerable<CookieType> _acceptedCookies;
        public bool ShoulAddEcommerceTags() {
            if (_acceptedCookies == null || !_acceptedCookies.Any()) {
                _acceptedCookies = _gdprScriptService.GetAcceptedCookieTypes();
            }
            var ecommerceCookieLevel = _orchardServices.WorkContext.CurrentSite.As<EcommerceAnalyticsSettingsPart>()?.EcommerceCookieLevel ?? CookieType.Statistical;
            return _acceptedCookies.Contains(ecommerceCookieLevel);
        }

        public bool UseGA4() {
            if (_gaSettings == null) {
                _gaSettings = _workContextAccessor.GetContext().CurrentSite.As<GoogleAnalyticsSettingsPart>();
            }

            if (_gaSettings == null) {
                return false;
            }

            // GA4 requires Tag Manager.
            return (_gaSettings.UseTagManager && _gaSettings.UseGA4);
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
                throw new ArgumentNullException("part.As<ProductPart>()");
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
            if (UseGA4()) {
                return GetJsonString(new GA4ProductVM(part));
            } else {
                return GetJsonString(new GTMProductVM(part));
            }
        }

        public string GetJsonString(IGAProductVM vm) {
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

        public IGAProductVM GetViewModel(GTMProductPart part) {
            if (UseGA4()) {
                return new GA4ProductVM(part);
            } else {
                return new GTMProductVM(part);
            }
        }

        #region private methods to handle tokenized fields
        // anything inserted into these tokens is thrown on the screen
        // even if there is html code
        private string FillString(string value, Dictionary<string, object> tokens) {
            if (!string.IsNullOrEmpty(value)) {
                // if a field like the bodypart with a lot of text is added, 
                // it is better to cut the string in order not to give problems to the page
                // the maximum length of the field is 255 characters, for the moment it's okay
                var maxLength = 255;
                var str = HttpUtility.HtmlDecode(_tokenizer.Replace(value, tokens));
                if (str.Length <= maxLength) {
                    return str;
                }
                else {
                    return str.Substring(0, maxLength);
                }
            }
            return string.Empty;
        }
        #endregion
    }
}