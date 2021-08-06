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
                checkResult.MessageError = "Config is missing";
                checkResult.Success = false;
            }
            else {
                try {
                    var url = "https://api.sandbox.paypal.com/v2/checkout/orders/" + oid;

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
                            if (id.Trim() == oid.Trim() && json["status"].ToString()=="COMPLETED") {
                                checkResult.Success = true;
                            }
                            else {
                                checkResult.Success = false;
                                if(id.Trim() != oid.Trim()) { 
                                    checkResult.MessageError = string.Format("Order id sent ({0}) does not match order verification id ({1})",oid.Trim(),id.Trim());
                                }

                                if(json["status"].ToString() != "COMPLETED") {
                                    checkResult.MessageError += string.Format("Status of order is not completed : {0}", json["status"].ToString());
                                }
                            }
                            checkResult.Info = stremReader;
                        }
                    }
                }
                catch (WebException e) {
                    checkResult.Success = false;
                    checkResult.MessageError = e.Message;
                }
                catch (Exception ex) {
                    checkResult.Success = false;
                    checkResult.MessageError = ex.Message;
                }
            }
            return checkResult;
        }

    }
}