using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using System.Web;

namespace Laser.Orchard.Mobile.Controllers {
    [WebApiKeyFilter(false)]
    public class DeviceController : ApiController {

        private readonly IOrchardServices _orchardServices;
        private readonly IPushNotificationService _pushNotificationService;
        public Localizer T { get; set; }

        public DeviceController(
             IOrchardServices orchardServices,
            IPushNotificationService pushNotificationService) {
            _orchardServices = orchardServices;
            _pushNotificationService = pushNotificationService;
            T = NullLocalizer.Instance;
        }

        public HttpResponseMessage Put(DeviceVM myDevice) {
            bool continuometodo = true;
            if (string.IsNullOrEmpty(myDevice.UUID))
                continuometodo = false;
            PushNotificationRecord devicetostore = new PushNotificationRecord();
            devicetostore.DataModifica = DateTime.Now;
            switch (myDevice.Device) {
                case "Apple":
                    if (myDevice.Token.Length > 100)
                        devicetostore.Device = TipoDispositivo.AppleFCM;
                    else
                        devicetostore.Device = TipoDispositivo.Apple;
                    break;
                case "Android":
                    devicetostore.Device = TipoDispositivo.Android;
                    break;
                case "WindowsMobile":
                    devicetostore.Device = TipoDispositivo.WindowsMobile;
                    break;
                default:
                    continuometodo = false;
                    break;
            }
            if (continuometodo) {
                try {
                    string host = HttpContext.Current.Request.Url.Host;

                    devicetostore.Validated = true;
                    devicetostore.Language = myDevice.Language;
                    devicetostore.Produzione = myDevice.Produzione;
                    devicetostore.Token = myDevice.Token;
                    devicetostore.UUIdentifier = myDevice.UUID;
                    _pushNotificationService.StorePushNotification(devicetostore);
                    HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    //// response.Headers.Location = new Uri(Request.RequestUri.ToString());
                    // message.Content = new StringContent( "OK");
                    //  throw new HttpResponseException(
                    return message;
                }
                catch (Exception ex) {
                    HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                    message.Content = new StringContent(ex.Message);
                    throw new HttpResponseException(message);
                }
            }
            else {
                HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                message.Content = new StringContent("No Device type or UUID specified.");
                throw new HttpResponseException(message);
            }
        }

        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // GET api/pushdata/5
        public string Get(int id) {
            return "value";
        }

        // POST api/pushdata
        public void Post([FromBody]string value) {
        }

        // PUT api/pushdata/5
        //public void Put(int id, [FromBody]string value) {

        //}

        // DELETE api/pushdata/5
        public void Delete(DeviceVM myDevice) {
            bool continuometodo = true;
            if (string.IsNullOrEmpty(myDevice.UUID))
                continuometodo = false;
            PushNotificationRecord devicetostore = new PushNotificationRecord();
            devicetostore.DataModifica = DateTime.Now;
            switch (myDevice.Device) {
                case "Apple":
                    if (myDevice.Token.Length > 100)
                        devicetostore.Device = TipoDispositivo.AppleFCM;
                    else
                        devicetostore.Device = TipoDispositivo.Apple;
                    break;
                case "Android":
                    devicetostore.Device = TipoDispositivo.Android;
                    break;
                case "WindowsMobile":
                    devicetostore.Device = TipoDispositivo.WindowsMobile;
                    break;
                default:
                    continuometodo = false;

                    break;
            }
            if (continuometodo) {
                try {
                    devicetostore.Validated = false;
                    devicetostore.Language = myDevice.Language;
                    devicetostore.Produzione = myDevice.Produzione;
                    devicetostore.Token = myDevice.Token;
                    devicetostore.UUIdentifier = myDevice.UUID;
                    _pushNotificationService.StorePushNotification(devicetostore);
                }
                catch (Exception ex) {
                    HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                    message.Content = new StringContent(ex.Message);
                    throw new HttpResponseException(message);
                }
            }
            else {
                HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                message.Content = new StringContent("No Device type or UUID specified.");
                throw new HttpResponseException(message);
            }
        }

    }
}