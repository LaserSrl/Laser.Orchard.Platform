using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.PartSettings;
using Newtonsoft.Json.Linq;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopService : IFacebookShopService {
        IWorkContextAccessor _workContext;
        ITokenizer _tokenizer;

        public FacebookShopService(
            IWorkContextAccessor workContext,
            ITokenizer tokenizer) {
            _workContext = workContext;
            _tokenizer = tokenizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CheckBusiness(FacebookShopServiceContext context) {
            string url = context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? context.BusinessId : "/" + context.BusinessId);

            url = string.Format(url + "?access_token={0}", GenerateAccessToken(context));

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

            url = string.Format(url + "?access_token={0}", GenerateAccessToken(context));

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

        public string GenerateAccessToken(FacebookShopServiceContext context) {
            // System user token should be saved in FacebookShopSiteSettingsPart.
            // It should have the required permissions to manage the catalog syncronization.
            return context.AccessToken;

            // Everything else is commented in case it's needed in the future.
            // Access code generation has the following steps:
            // 1. Authorize the client: https://graph.facebook.com/v11.0/oauth/authorize?client_id=1534966686844583&scope=business_management,catalog_management&redirect_uri=https://win2016dev.westeurope.cloudapp.azure.com/OrchardPiovanelliA/TestLite
            // 2. The previous call redirects to: https://win2016dev.westeurope.cloudapp.azure.com/OrchardPiovanelliA/TestLite?code=AQCe-_0L4pQGdvCVWApdknt03o2WttfmaTKkpUL7CJZjOeXEXUm62D6rkVzGN7fLRJqn7eYAHQ5cn_c7gfOzJDE2DXJPxjyWbphMrW1_WwSCFS6i3SWuZABo7xV8-NNAyBCdKF85ZydTV7A9UdecM6Iydkelxs0WzUX3IxI49A9ViZv78octsZCBRjHo5z0w9OaXDQGY56okJ8NuFvtCYkeDuKTu37Su7p2Jp7ONRGRGFii0hKAIADtFGw98DtVvBHQ33EebAau5TT1cPoBl1IHaiT2oUwQAsNaaUDBMo8KX0IPFPLd_bvEceMpx8stXIp9vIQek32yiNFRXQ06pt6Qo#_=_
            // We need to save the parameter "code" from the previous query string (for this, we need a proper Controller).
            // 3. At this point, with the code, we can generate the access_token:
            // https://graph.facebook.com/v11.0/oauth/access_token?client_id=1534966686844583&scope=business_management,catalog_management&redirect_uri=https://win2016dev.westeurope.cloudapp.azure.com/OrchardPiovanelliA/TestLite&code=AQAdwntGxdiyn2ixmKjhARWQqmsVtiGHx8vSM5VxOeJwYhTPB17kU1ZTRx-qQ81daYlcxGaNO8hRnEa56nRrQztmZ1YViHvIQoatfLJS3b_gk9uYMu8lieBXxzIm5Q72z0VCrSeJ1K0AArzF19PzRPksNx1xO9Mt_ZgSwLapZrf4cz8yLV82OShxSpeZH_SknR5YvXAgNrF9-26sJ63cfNmm6A4XpzwO60jS3yrp0VGNCGf1Bt9T1keRi5F5Mo1nfM9Eopxih01fXh6nlq2iKytnxIZJ6vapg0iK8NhD_WL2r3c8sPuk52gpVv3HH3bQkq17JFYetOrvHWoTiCFDRmRr%23_=_&client_secret=cebbdd99828772a89f9cf4ce93754cbf#_=_
            // Response of the previous call is in the following format:
            //{
            //  "access_token": "EAAV0C0gchqcBADm8ZBOgHI3hGAaa90ApypIxjHBPVHIQt6m0Dw4U16HJdQvZCS6agaTJTRdOMGlAtNlrIn255ZCvZB8Nna2BmiH85zvXAdvwf7nZC9k4yPfafyXGUUW1LbMQ8136NOZClFJN1ovV4dQbWJ6mvlMt9NuRnGQNspu8FEJves6QXG",
            //  "token_type": "bearer",
            //  "expires_in": 5181913 // 60 days
            //}

            //string scope = "business_management,catalog_management";

            //string redirect_uri = "https://win2016dev.westeurope.cloudapp.azure.com/OrchardPiovanelliA/TestLite";
            //string url = string.Format(
            //        "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}",
            //        context.AppId, redirect_uri, scope);

            //// SysAdmin token
            //var token = "EAAV0C0gchqcBALZAxI0y2rfzvR1v7AxXYxZA6hb2XRPIURuJGGRP5rAYCdlXJEPQkNJG5WWdwf7e3A7rLLQTBM6GucBX4aazKZAZALFCbM93BN1tdValDcnGBJRQ4kcZBQil9zv4e82pKQ7yuRMOX9MH3OnZB4GTgr0ag8bVCKgsLqcT3RHUtl";

            //return token;

            //return "EAAV0C0gchqcBACLwPhZC28TcidhU31whL9X1rUuDNRsWeofh15mUcHSPvPcw4njk2cafq0UGXSdlPxXCgcYDtLb58chLt0vPEkMPpsEP7Nf56fOWIGea9J2EL3NEjetBcxSmwjESIA8XHMI6vkG9Dp2BcKjZBZBQ6VAa67J1B486QRb196732lyEdaEwfEZASZAi6CDoyX2gzxe9eQQfyG7dsNYeRt5pUirs14ZBls3Y3MNtlZAwOJW";

            // Commented because app access token doesn't have enough permissions.
            //string grant_type = "client_credentials";
            //string url = string.Format(context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? "" : "/") + 
            //    "oauth/access_token?client_id={0}&client_secret={1}&grant_type={2}&redirect_uri={3}",
            //        context.AppId, context.AppSecret, grant_type, "");

            //HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;
            //string access_token = "";
            //try {
            //    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
            //        StreamReader reader = new StreamReader(response.GetResponseStream());
            //        string vals = reader.ReadToEnd();
            //        var json = JObject.Parse(vals);
            //        if ((json["access_token"]).Type != JTokenType.Null)
            //            access_token = (json["access_token"] ?? "").ToString();
            //    }
            //} catch (Exception ex) {
            //    return "EAAV0C0gchqcBAC9SYZCve2PQm1AB1ZAlIZBynwoKAUHKdpCR2ME9gWjUKcq3m4hCfbv8ZCSc2mAtbmYqEQroq9EPEF8raZBuSkLkMOBDEvtD8HOPwiZAShxuBAe5Xc9NzpZA514LJaBe0RzHA4L9U4ZAU7pPLKfNZB1VRr4autBdVPAXM6AEEU2GLDl7Rx60lttYkYC7XFP5wcjpZAwcy8zYHN663GdWvtKWpAbp6vRU8Y3ZAorCGvL7FrE";
            //}

            //return access_token;
        }

        public FacebookShopProductUpdateRequest SyncProduct(ContentItem product) {
            try {
                var productPart = product.As<ProductPart>();
                var facebookPart = product.As<FacebookShopProductPart>();

                if (productPart != null && facebookPart != null && facebookPart.SynchronizeFacebookShop) {
                    var jsonTemplate = facebookPart.Settings.GetModel<FacebookShopProductPartSettings>().JsonForProductUpdate;
                    var fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
                    if (string.IsNullOrWhiteSpace(jsonTemplate)) {
                        // Fallback to FacebookShopSiteSettingsPart
                        jsonTemplate = fsssp.DefaultJsonForProductUpdate;
                    }

                    if (!string.IsNullOrWhiteSpace(jsonTemplate)) {
                        // jsonTemplate typically begins with a double '{' and ends with a double '}' (to make tokens work).
                        // For this reason, before deserialization, I need to replace tokens and replace double parenthesis.
                        string jsonBody = _tokenizer.Replace(jsonTemplate, product);
                        jsonBody = jsonBody.Replace("{{", "{").Replace("}}", "}");

                        var jsonContext = FacebookShopProductUpdateRequest.From(jsonBody);

                        CheckCompliance(jsonContext, product);

                        if (jsonContext != null && jsonContext.Valid) {
                            return PostProduct(jsonContext);
                        } else {
                            // I need to tell it was impossible to synchronize the product on Facebook Shop.
                            return new FacebookShopProductUpdateRequest() {
                                Message = T("Facebook shop synchronization failed."),
                                Valid = false
                            };
                        }
                    }
                }
            } catch {
                // I need to tell it was impossible to synchronize the product on Facebook Shop.
                return new FacebookShopProductUpdateRequest() {
                    Message = T("Facebook shop synchronization failed."),
                    Valid = false
                };
            }

            return new FacebookShopProductUpdateRequest() {
                Message = T("Facebook shop synchronization failed."),
                Valid = false
            };
        }

        private string EncapsulateUpdateProductJson(string jsonStr) {
            return "{\"requests\":[" + jsonStr + "]}";
        }

        public FacebookShopProductUpdateRequest PostProduct(FacebookShopProductUpdateRequest context) {
            FacebookShopRequestContainer requestContainer = new FacebookShopRequestContainer();
            requestContainer.Requests.Add(context);
                        
            // Facebook Shop Site Settings: I need url, catalog id and access token.
            var fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
            string url = fsssp.ApiBaseUrl + (fsssp.ApiBaseUrl.EndsWith("/") ? fsssp.CatalogId : "/" + fsssp.CatalogId);
            FacebookShopServiceContext ctx = new FacebookShopServiceContext() {
                ApiBaseUrl = fsssp.ApiBaseUrl,
                BusinessId = fsssp.BusinessId,
                CatalogId = fsssp.CatalogId,
                AccessToken = fsssp.AccessToken
            };
            url = string.Format(url + "/batch?access_token={0}", GenerateAccessToken(ctx));

            var jsonBody = requestContainer.ToJson();

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

                            // If I'm here, product should be on Facebook Shop.
                            context.Valid = true;
                            return context;
                        }
                    } else {
                        context.Valid = false;
                        context.Message = T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.");
                    }
                }
            } catch {
                context.Valid = false;
                context.Message = T("Invalid Facebook api response. Product is not synchronized on Facebook Shop.");
            }

            return context;
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

            if (string.IsNullOrWhiteSpace(context.Availability)) {
                context.Availability = "in stock";
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
                decimal price = product.As<ProductPart>().ProductPriceService.GetPrice(product.As<ProductPart>());

                int cents = (int)(price * 100);
                context.Price = cents.ToString();
            }
            // End of price analysis.

            if (string.IsNullOrWhiteSpace(context.Currency)) {
                context.Currency = "EUR";
            }

            if (string.IsNullOrWhiteSpace(context.Url)) {
                // Url is empty, I need to create it from scratch.
                UrlHelper urlHelper = new UrlHelper(_workContext.GetContext().HttpContext.Request.RequestContext);
                // www.mysite.com/route-to-contentitem
                // WARNING: if you're on localhost/apppool/tenant..., this doesn't work because it duplicates the app pool.
                // BaseUrl is something like http://localhost/apppool/
                // RouteUrl is something like /apppool/route-to-contentitem
                context.Url = _workContext.GetContext().CurrentSite.BaseUrl + urlHelper.RouteUrl(product.ContentManager.GetItemMetadata(product).DisplayRouteValues);
            }

            if (!context.Url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !context.Url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)) {
                // Url isn't a complete url (it doesn't start with http:// or https://).
                // I need to add the BaseUrl.
                context.Url = _workContext.GetContext().CurrentSite.BaseUrl + context.Url;
            }

            if (string.IsNullOrWhiteSpace(context.ImageUrl)) {
                context.Message = T("Invalid image url");
                context.Valid = false;
                return context;
            }

            if (string.IsNullOrWhiteSpace(context.Brand)) {
                // TODO: what's the default brand of a product?
                context.Brand = _workContext.GetContext().CurrentSite.SiteName;
            }

            return context;
        }
    }
}