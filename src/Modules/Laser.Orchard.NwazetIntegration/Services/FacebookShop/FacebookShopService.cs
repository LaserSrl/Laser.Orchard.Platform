using Laser.Orchard.NwazetIntegration.Handlers;
using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.PartSettings;
using Newtonsoft.Json.Linq;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using Orchard.Tokens;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopService : IFacebookShopService {
        private readonly IWorkContextAccessor _workContext;
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IScheduledTaskManager _taskManager;
        private FacebookShopSiteSettingsPart _fsssp;
        private readonly IClock _clock;
        private readonly IRepository<FacebookShopHandleRecord> _handles;

        public FacebookShopService(
            IWorkContextAccessor workContext,
            ITokenizer tokenizer,
            IContentManager contentManager,
            ICurrencyProvider currencyProvider,
            IScheduledTaskManager taskManager,
            IClock clock,
            IRepository<FacebookShopHandleRecord> handles) {
            _workContext = workContext;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _currencyProvider = currencyProvider;
            _taskManager = taskManager;
            _clock = clock;
            _handles = handles;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool CheckBusiness(FacebookShopServiceContext context) {
            string url = context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? context.BusinessId : "/" + context.BusinessId);

            url = string.Format(url + "?access_token={0}", context.AccessToken);

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            try {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    var json = JObject.Parse(reader.ReadToEnd());
                    if (json["error"] == null && json["id"].ToString().Equals(context.BusinessId, System.StringComparison.InvariantCultureIgnoreCase)) {
                        return true;
                    }
                }
            } catch {
                return false;
            }
            return false;
        }

        public bool CheckCatalog(FacebookShopServiceContext context) {
            string url = context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? context.CatalogId : "/" + context.CatalogId);

            url = string.Format(url + "?access_token={0}", context.AccessToken);

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            try {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    var json = JObject.Parse(reader.ReadToEnd());
                    if (json["error"] == null && json["id"].ToString().Equals(context.CatalogId, System.StringComparison.InvariantCultureIgnoreCase)) {
                        return true;
                    }
                }
            } catch {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Creates a scheduled task to synchronize products on Facebook Shop.
        /// </summary>
        public void ScheduleProductSynchronization() {
            _taskManager.CreateTask(FacebookShopProductSynchronizationTaskHandler.SYNCPRODUCTS_TASK, _clock.UtcNow.AddMinutes(1), null);
        }

        public FacebookShopRequestContainer SyncProduct(ContentItem product) {
            var productPart = product.As<ProductPart>();
            try {
                var facebookPart = product.As<FacebookShopProductPart>();

                if (productPart != null && facebookPart != null && facebookPart.SynchronizeFacebookShop) {
                    var jsonTemplate = facebookPart.Settings.GetModel<FacebookShopProductPartSettings>().JsonForProductUpdate;
                    _fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
                    if (string.IsNullOrWhiteSpace(jsonTemplate)) {
                        // Fallback to FacebookShopSiteSettingsPart
                        jsonTemplate = _fsssp.DefaultJsonForProductUpdate;
                    }

                    if (!string.IsNullOrWhiteSpace(jsonTemplate)) {
                        // jsonTemplate typically begins with a double '{' and ends with a double '}' (to make tokens work).
                        // For this reason, before deserialization, I need to replace tokens and replace double parenthesis.
                        string jsonBody = _tokenizer.Replace(jsonTemplate, product);
                        jsonBody = jsonBody.Replace("{{", "{").Replace("}}", "}");

                        var jsonContext = FacebookShopProductUpdateRequest.From(jsonBody);

                        CheckCompliance(jsonContext, product);

                        if (jsonContext != null && jsonContext.Valid) {
                            return SyncProduct(jsonContext);
                        } else if (jsonContext != null) {
                            // I need to tell it was impossible to synchronize the product on Facebook Shop.
                            Logger.Debug(T("Product {0} can't be synchronized on Facebook catalog.", productPart.Sku).Text);
                            Logger.Debug(jsonContext.Message.Text);

                            var returnValue = new FacebookShopRequestContainer();
                            returnValue.Requests.Add(jsonContext);
                            return returnValue;
                        }
                    }
                }
            } catch (Exception ex) {
                // I need to tell it was impossible to synchronize the product on Facebook Shop.
                if (productPart != null) {
                    Logger.Debug(ex, T("Product {0} can't be synchronized on Facebook catalog.", productPart.Sku).Text);
                } else {
                    Logger.Debug(ex, T("Product part or Facebook part are not valid.").Text);
                }
                return null;
            }

            return null;
        }

        public FacebookShopRequestContainer SyncProduct(FacebookShopProductUpdateRequest context) {
            FacebookShopRequestContainer requestContainer = new FacebookShopRequestContainer();
            requestContainer.Requests.Add(context);

            // Facebook Shop Site Settings: I need url, catalog id and access token.
            _fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
            FacebookShopServiceContext ctx = new FacebookShopServiceContext() {
                ApiBaseUrl = _fsssp.ApiBaseUrl,
                BusinessId = _fsssp.BusinessId,
                CatalogId = _fsssp.CatalogId,
                AccessToken = _fsssp.AccessToken
            };

            FacebookShopProductBatch(requestContainer);

            return requestContainer;
        }

        public FacebookShopRequestContainer RemoveProduct(ContentItem product) {
            var productPart = product.As<ProductPart>();
            var facebookPart = product.As<FacebookShopProductPart>();
            try {
                if (productPart != null && facebookPart != null && facebookPart.SynchronizeFacebookShop) {
                    // I need to assign RetailerId parameter to my context.
                    var context = new FacebookShopProductDeleteRequest() {
                        Method = FacebookShopProductDeleteRequest.METHOD,
                        Valid = true,
                        RetailerId = productPart.Sku
                    };

                    return RemoveProduct(context);
                }
            } catch (Exception ex) {
                // I need to tell it was impossible to synchronize the product on Facebook Shop.
                if (productPart != null) {
                    Logger.Debug(ex, T("Product {0} can't be removed from Facebook catalog.", productPart.Sku).Text);
                } else {
                    Logger.Debug(ex, T("Product part or Facebook part are not valid.").Text);
                }
                return null;
            }

            return null;
        }

        public FacebookShopRequestContainer RemoveProduct(FacebookShopProductDeleteRequest context) {
            FacebookShopRequestContainer requestContainer = new FacebookShopRequestContainer();
            requestContainer.Requests.Add(context);

            // Facebook Shop Site Settings: I need url, catalog id and access token.
            _fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
            FacebookShopServiceContext ctx = new FacebookShopServiceContext() {
                ApiBaseUrl = _fsssp.ApiBaseUrl,
                BusinessId = _fsssp.BusinessId,
                CatalogId = _fsssp.CatalogId,
                AccessToken = _fsssp.AccessToken
            };

            FacebookShopProductBatch(requestContainer);

            return requestContainer;
        }

        public void SyncProducts() {
            // Facebook Shop Site Settings: I need url, catalog id and access token.
            _fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
            FacebookShopServiceContext ctx = new FacebookShopServiceContext() {
                ApiBaseUrl = _fsssp.ApiBaseUrl,
                BusinessId = _fsssp.BusinessId,
                CatalogId = _fsssp.CatalogId,
                AccessToken = _fsssp.AccessToken
            };

            // Query on published products, to send them all in a single request to Facebook api.
            // Every call to Facebook api sends the update of 20 products.
            int step = 20;
            // I look for the FacebookShopPart because I may have some products I don't need to synchronize on Facebook Shop (without the FacebookShopProductPart or with the SynchronizeFacebookShopFlag disabled).
            var query = _contentManager.Query<FacebookShopProductPart, FacebookShopProductPartRecord>(VersionOptions.Published)
                .Where(fp => fp.SynchronizeFacebookShop == true);

            for (int count = 0; count < query.Count(); count += step) {
                var facebookParts = _contentManager.Query<FacebookShopProductPart, FacebookShopProductPartRecord>(VersionOptions.Published)
                    .Where(fp => fp.SynchronizeFacebookShop == true)
                    .Slice(count, step);

                // I build a container for each slice of the query results.
                FacebookShopRequestContainer requestContainer = new FacebookShopRequestContainer();

                foreach (var facebookPart in facebookParts) {
                    var jsonContext = GetJsonContext(facebookPart.ContentItem);

                    if (jsonContext != null && jsonContext.Valid) {
                        requestContainer.Requests.Add(jsonContext);
                    } else if (jsonContext != null) {
                        Logger.Error(T("Product {0} can't be synchronized on Facebook catalog.", jsonContext.RetailerId).Text);
                        Logger.Error(jsonContext.Message.Text);
                    }
                }

                // I can now send the request to Facebook api.
                // This call contains every valid product in the slice from the query.
                FacebookShopProductBatch(requestContainer);
            }
        }

        /// <summary>
        /// Calls Facebook Api batch endpoint.
        /// </summary>
        /// <param name="container"></param>
        private void FacebookShopProductBatch(FacebookShopRequestContainer container) {
            if (container.Requests.Count == 0) {
                Logger.Debug(T("No products in request container.").Text);
            } else {
                var jsonBody = container.ToJson();
                string url = _fsssp.ApiBaseUrl + (_fsssp.ApiBaseUrl.EndsWith("/") ? _fsssp.CatalogId : "/" + _fsssp.CatalogId);
                url = string.Format(url + "/batch?access_token={0}", _fsssp.AccessToken);

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                try {
                    byte[] bodyData = Encoding.UTF8.GetBytes(jsonBody);
                    request.Method = WebRequestMethods.Http.Post;
                    request.ContentType = "application/json";

                    using (Stream reqStream = request.GetRequestStream()) {
                        reqStream.Write(bodyData, 0, bodyData.Length);
                    }

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                        if (response.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new StreamReader(response.GetResponseStream())) {
                                string respJson = reader.ReadToEnd();
                                var json = JObject.Parse(respJson);

                                // Facebook handles are managed after a while (we can't read errors immediately after the product batch call).
                                // For this reason, I need to schedule the CheckFacebookHandles call.
                                // First I need to create a FacebookShopHandleRecord.
                                if (json["handles"] != null) {
                                    var handleId = CreateFacebookShopHandleRecord(json["handles"].First.ToString(), jsonBody);
                                    _taskManager.CreateTask(FacebookShopProductSynchronizationTaskHandler.CHECKHANDLE_TASK + "_" + handleId.ToString(), _clock.UtcNow.AddMinutes(1), null);
                                }

                                // If I'm here, product/s should be on Facebook Shop.
                                if (container.Requests.Count == 1) {
                                    Logger.Debug(T("Product {0} synchronized on Facebook Shop.", container.Requests[0].RetailerId).Text);
                                } else {
                                    Logger.Debug(T("{0} products synchronized on Facebook Shop.", container.Requests.Count.ToString()).Text);
                                }
                            }
                        } else {
                            Logger.Debug(T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.").Text);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Debug(ex, T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.").Text);
                }
            }
        }

        private int CreateFacebookShopHandleRecord(string handle, string requestJson) {
            var rec = new FacebookShopHandleRecord() {
                RequestJson = requestJson,
                Handle = handle,
                Processed = false
            };
            _handles.Create(rec);
            return rec.Id;
        }

        public void CheckFacebookHandles(string handle, string containerJson) {
            if (!string.IsNullOrWhiteSpace(handle)) {
                _fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
                string url = _fsssp.ApiBaseUrl + (_fsssp.ApiBaseUrl.EndsWith("/") ? _fsssp.CatalogId : "/" + _fsssp.CatalogId);
                url = string.Format(url + "/check_batch_request_status?handle={0}&access_token={1}", handle, _fsssp.AccessToken);
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                try {
                    request.Method = WebRequestMethods.Http.Get;

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                        if (response.StatusCode == HttpStatusCode.OK) {
                            using (var reader = new StreamReader(response.GetResponseStream())) {
                                string respJson = reader.ReadToEnd();
                                var handlesJson = JObject.Parse(respJson);
                                var errors = handlesJson["data"].First["errors"];
                                if (errors != null) {
                                    var error = errors.First;
                                    if (error != null) {
                                        // Quoted for the moment, it can be useful in the future if we want to have more structured intel on Facebook handles.
                                        //var line = error["line"];
                                        //var productSku = error["id"];
                                        //var message = error["message"];

                                        var errorLog = T("Error when checking Facebook Shop handle").Text;
                                        errorLog += Environment.NewLine + containerJson;
                                        errorLog += Environment.NewLine + handlesJson.ToString();
                                        Logger.Error(errorLog);

                                        // Quoted for the moment, it can be useful in the future if we want to have more structured intel on Facebook handles.
                                        //while (error != null) {
                                        //    error = error.Next;

                                        //    if (error != null) {
                                        //        line = error["line"];
                                        //        productSku = error["id"];
                                        //        message = error["message"];
                                        //    }
                                        //}
                                    }
                                }
                            }
                        } else {
                            Logger.Debug(T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.").Text);
                        }
                    }
                } catch (Exception ex) {
                    Logger.Debug(ex, T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.").Text);
                }
            }
        }

        private IFacebookShopRequest GetJsonContext(ContentItem product) {
            var facebookPart = product.As<FacebookShopProductPart>();
            var productPart = product.As<ProductPart>();
            // Content Item must be a product with the FacebookShopProductPart.
            if (productPart != null && facebookPart != null) {
                var jsonTemplate = facebookPart.Settings.GetModel<FacebookShopProductPartSettings>().JsonForProductUpdate;
                if (string.IsNullOrWhiteSpace(jsonTemplate)) {
                    // Fallback to FacebookShopSiteSettingsPart
                    jsonTemplate = _fsssp.DefaultJsonForProductUpdate;
                }

                // jsonTemplate typically begins with a double '{' and ends with a double '}' (to make tokens work).
                // For this reason, before deserialization, I need to replace tokens and replace double parenthesis.
                string productJson = _tokenizer.Replace(jsonTemplate, facebookPart.ContentItem);
                productJson = productJson.Replace("{{", "{").Replace("}}", "}");

                var jsonContext = FacebookShopProductUpdateRequest.From(productJson);

                CheckCompliance(jsonContext, facebookPart.ContentItem);

                if (jsonContext != null && !jsonContext.Valid) {
                    Logger.Debug(jsonContext.Message.Text);
                }

                return jsonContext;
            } else {
                Logger.Debug(T("Invalid Product part or Facebook part.").Text);
            }

            return null;
        }

        /// <summary>
        /// This routine checks the compliance of this class with Facebook Shop standards, verifying that default fields are compiled.
        /// It eventually looks for defaults for fields that have been left empty.
        /// </summary>
        private FacebookShopProductUpdateRequest CheckCompliance(FacebookShopProductUpdateRequest context, ContentItem product) {
            if (string.IsNullOrWhiteSpace(context.Method)) {
                context.Method = FacebookShopProductUpdateRequest.METHOD;
            }

            if (string.IsNullOrWhiteSpace(context.RetailerId)) {
                context.RetailerId = product.As<ProductPart>().Sku;
            }

            if (context.ProductData == null) {
                context.ProductData = new FacebookServiceJsonContextData() {
                    Valid = true
                };
            }

            CheckCompliance(context.ProductData, product);

            if (context.ProductData == null || !context.ProductData.Valid) {
                if (context.ProductData != null && context.ProductData.Message != null) {
                    context.Message = context.ProductData.Message;
                } else {
                    context.Message = T("Invalid data");
                }
                context.Valid = false;
            }

            return context;
        }

        private bool IsAllUpper(string str) {
            for (int i = 0; i < str.Length; i++) {
                if (Char.IsLetter(str[i]) && !Char.IsUpper(str[i]))
                    return false;
            }
            return true;
        }

        private FacebookServiceJsonContextData CheckCompliance(FacebookServiceJsonContextData context, ContentItem product) {
            if (string.IsNullOrWhiteSpace(context.Name)) {
                context.Name = product.ContentManager.GetItemMetadata(product).DisplayText;
            }

            if (string.IsNullOrWhiteSpace(context.Description)) {
                var bodyPart = product.As<BodyPart>();
                if (bodyPart == null) {
                    context.Message = T("Invalid product description (body part not found)");
                    context.Valid = false;
                    return context;
                } else {
                    context.Description = bodyPart.Text;
                }
            }

            if (IsAllUpper(context.Description)) {
                context.Message = T("Invalid product description (description must not be all upper case)");
                context.Valid = false;
                return context;
            }

            var productPart = product.As<ProductPart>();

            if (string.IsNullOrWhiteSpace(context.Availability)) {
                // I need to consider inventory and minimum order quantity.
                // If product is digital and I don't need to consider inventory, product is always in stock.
                if (productPart.IsDigital && !productPart.ConsiderInventory) {
                    context.Availability = "in stock";
                } else if (productPart.Inventory > 0 && productPart.Inventory >= productPart.MinimumOrderQuantity) {
                    context.Availability = "in stock";
                } else {
                    context.Availability = "out of stock";
                }
            }

            if (string.IsNullOrWhiteSpace(context.Condition)) {
                context.Condition = "new";
            }

            // The price in the product section of Facebook Shop json is the number of cents.
            // E.g.: if the price is 9.99, we need to write 999 in the json.
            bool getDefaultPrice = false;
            decimal decPrice = 0;

            if (string.IsNullOrWhiteSpace(context.Price)) {
                getDefaultPrice = true;
            } else if (decimal.TryParse(context.Price, out decPrice)) {
                context.Price = ((int)(decPrice * 100)).ToString();
            } else {
                getDefaultPrice = true;
            }

            if (getDefaultPrice) {
                decimal price = productPart.ProductPriceService.GetPrice(productPart);

                int cents = (int)(price * 100);
                context.Price = cents.ToString();
            }
            // End of price analysis.

            if (string.IsNullOrWhiteSpace(context.Currency)) {
                context.Currency = _currencyProvider.CurrencyCode;
            }

            if (string.IsNullOrWhiteSpace(context.Url)) {
                // Url is empty, I need to create it from scratch.
                UrlHelper urlHelper = new UrlHelper(_workContext.GetContext().HttpContext.Request.RequestContext);
                // www.mysite.com/route-to-contentitem
                // WARNING: if you're on localhost/apppool/tenant..., this doesn't work because it duplicates the app pool for the following reason:
                // BaseUrl is something like http://localhost/apppool/
                // RouteUrl is something like /apppool/route-to-contentitem
                context.Url = _workContext.GetContext().CurrentSite.BaseUrl + urlHelper.RouteUrl(product.ContentManager.GetItemMetadata(product).DisplayRouteValues);
            }

            if (!context.Url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !context.Url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)) {
                // If url isn't a complete url (it doesn't start with http:// or https://).
                // I need to add the BaseUrl.
                context.Url = _workContext.GetContext().CurrentSite.BaseUrl + context.Url;
            }

            if (string.IsNullOrWhiteSpace(context.ImageUrl)) {
                // TODO: check for the default image url.
                context.Message = T("Invalid image url");
                context.Valid = false;
                return context;
            }

            if (string.IsNullOrWhiteSpace(context.Brand)) {
                // TODO: what's the default brand of a product?
                // For the moment, default brand of a product is assumed to be the SiteName.
                context.Brand = _workContext.GetContext().CurrentSite.SiteName;
            }

            return context;
        }
    }
}