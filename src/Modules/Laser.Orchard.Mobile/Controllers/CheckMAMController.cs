using Laser.Orchard.Mobile.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Environment.Extensions;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {

    [Authorize]
    [WebApiKeyFilter(true)]
    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class CheckMAMController : ApiController {

        private readonly ISmsServices _smsServices;

        public CheckMAMController(ISmsServices smsServices) {
            _smsServices = smsServices;
        }

        /// <summary>
        /// Metodo per ritornare lo stato della MAM - 0 NOT OK / 1 OK
        /// http://localhost/Laser.Orchard/Sviluppo/Api/Laser.Orchard.Mobile/CheckMAM
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public int Get() {
            return _smsServices.GetStatus();
        }

        public void Post() { }

        public void Put() { }

        public void Delete() { }
    }
}