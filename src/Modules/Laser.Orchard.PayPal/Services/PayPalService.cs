using Laser.Orchard.PayPal.Models;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Laser.Orchard.PayPal.Services {
    public class PayPalService : IPayPalService {
        private readonly IOrchardServices _orchardServices;

        public PayPalService(
            IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public CheckOrderResult VerifyOrderIdPayPal(string oid) {
            var checkResult = new CheckOrderResult();

            var config = _orchardServices.WorkContext.CurrentSite.As<PayPalSiteSettingsPart>();
            if (string.IsNullOrWhiteSpace(config.SecretId) || string.IsNullOrWhiteSpace(config.ClientId)) {
                checkResult.Status = "Config is missing";
                checkResult.Error = true;
            }
            else {
                try {
                    var url = "https://api.sandbox.paypal.com/v2/checkout/orders/" + oid;

                    // specify to use TLS 1.2 as default connection if needed
                    if (url.ToLowerInvariant().StartsWith("https:")) {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    }

                    HttpWebRequest request;
                    HttpWebResponse response;

                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.ContentType = "application/json; charset=utf-8";
                    request.Method = "GET";

                    var byteArr = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", config.ClientId, config.SecretId));

                    String encoded = Convert.ToBase64String(byteArr);
                    request.Headers.Add("Authorization", "Basic " + encoded);

                    response = (HttpWebResponse)request.GetResponse();
                    using (var streamReader = new StreamReader(response.GetResponseStream())) {
                        var stremReader = streamReader.ReadToEnd();
                        if (streamReader != null) {
                            var json = JObject.Parse(stremReader);
                            var id = json["id"] == null ? string.Empty : json["id"].ToString();
                            checkResult.OrderIdReturned = id;
                            checkResult.Status = json["status"].ToString(); // Completed
                            checkResult.Error = false;
                        }
                    }
                }
                catch (WebException e) {
                    checkResult.Error = true;
                    checkResult.Status = e.Message;
                }
                catch (Exception ex) {
                    checkResult.Error = true;
                    checkResult.Status = ex.Message;
                }
            }
            return checkResult;
        }

    }
}