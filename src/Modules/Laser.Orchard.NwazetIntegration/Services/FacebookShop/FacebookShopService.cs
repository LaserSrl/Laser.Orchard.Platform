using Newtonsoft.Json.Linq;
using Orchard.Environment.Extensions;
using System;
using System.IO;
using System.Net;

namespace Laser.Orchard.NwazetIntegration.Services.FacebookShop {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopService : IFacebookShopService {
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
            } catch (Exception ex) {
                return false;
            }
            return false;
        }

        public bool CheckCatalog(FacebookShopServiceContext context) {
            string url = context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? context.CatalogId : "/" + context.BusinessId);

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
            } catch (Exception ex) {
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
    }
}