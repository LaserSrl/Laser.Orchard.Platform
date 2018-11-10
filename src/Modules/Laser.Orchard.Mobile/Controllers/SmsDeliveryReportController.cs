using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {

    [Authorize]
    [WebApiKeyFilter(true)]
    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsDeliveryReportController : ApiController {

        private readonly ISmsServices _smsServices;
        private readonly IRepository<SmsGatewayPartRecord> _smsGatewayRepository;

        public SmsDeliveryReportController(ISmsServices smsServices, IRepository<SmsGatewayPartRecord> smsGatewayRepository) {
            _smsServices = smsServices;
            _smsGatewayRepository = smsGatewayRepository;
        }

        /// <summary>
        /// Metodo per ritornare le informazioni sul dettaglio della spedizione
        /// http://localhost/Laser.Orchard/Sviluppo/Api/Laser.Orchard.Mobile/SmsDeliveryReport?ExternalId={id}
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public SmsDeliveryReportResultVM Get(string ExternalId) {
            // Recupero Id spedizione tramite ExternalId
            int id = _smsGatewayRepository.Fetch(x => x.ExternalId == ExternalId).FirstOrDefault().Id;
            string smsId = "Orchard_" + id.ToString();

            return _smsServices.GetReportSmsStatus(smsId);
        }

        public void Post() { }

        public void Put() { }

        public void Delete() { }

    }
}