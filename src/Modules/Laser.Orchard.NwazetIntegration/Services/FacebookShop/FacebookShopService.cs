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
            return "EAAV0C0gchqcBACLwPhZC28TcidhU31whL9X1rUuDNRsWeofh15mUcHSPvPcw4njk2cafq0UGXSdlPxXCgcYDtLb58chLt0vPEkMPpsEP7Nf56fOWIGea9J2EL3NEjetBcxSmwjESIA8XHMI6vkG9Dp2BcKjZBZBQ6VAa67J1B486QRb196732lyEdaEwfEZASZAi6CDoyX2gzxe9eQQfyG7dsNYeRt5pUirs14ZBls3Y3MNtlZAwOJW";

            // Commented because app access token doesn't have enough permissions.
            //string grant_type = "client_credentials";
            //string url= string.Format(context.ApiBaseUrl + (context.ApiBaseUrl.EndsWith("/") ? "" : "/") + 
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