using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {
    /// <summary>
    /// ApiController con abilitazione CORS.
    /// Per permettere la fase di preflight request, la protezione WebApi non è a livello di classe ma di metodo, eccetto il metodo Options.
    /// </summary>
    public class SignalApiController : ApiController {
        private readonly IActivityServices _activityServices;
        private readonly IOrchardServices _orchardServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUtilsServices _utilsServices;

        public SignalApiController(IOrchardServices orchardServices, IActivityServices activityServices, ICsrfTokenHelper csrfTokenHelper, IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _activityServices = activityServices;
            _csrfTokenHelper = csrfTokenHelper;
            _utilsServices = utilsServices;
        }

        /// <summary>
        /// Metodo necessario per rendere accessibile questo ApiController da browser nel caso di chiamate CORS (Cross-Origin Resource Sharing), quindi da un altro dominio.
        /// Questo metodo viene chiamato dai browser nella fase di preflight request.
        /// </summary>
        /// <returns></returns>
        public string Options() {
            var httpResponse = HttpContext.Current.Response;
            httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            httpResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            httpResponse.Headers.Add("Access-Control-Allow-Headers", "AKIV");
            return "";
        }

        /// <summary>
        /// Trigger the Workflow Signal defined by its Name over the specified ContentItem
        /// </summary>
        /// <param name="signal">an object representing a Signal.
        /// Example: 
        /// Request Header
        ///     Content-Type:application/x-www-form-urlencoded
        /// Request Body
        ///     Name:BookParkingPlace
        ///     ContentId:1118
        ///     ... other custom properties can be added here
        /// </param>
        /// <returns>returns a Response Object</returns>
        [WebApiKeyFilter(true)]
        [OutputCache(NoStore = true, Duration = 0)]
        public Response Post([FromBody] Signal signal, string signalName = "") {
            if (!string.IsNullOrWhiteSpace(signalName)) {
                signal.Name = signalName;//the signalName parameter overrides the body Signal.Name
            }
            if (String.IsNullOrWhiteSpace(signal.Name) || signal.ContentId <= 0) {
                throw new Exception("Invalid Signal parameters");
            }
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                if (!_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                    return _utilsServices.GetResponse(ResponseType.InvalidXSRF);
                }

            }
            // aggiunge l'header Access-Control-Allow-Origin necessario nel caso di chiamate CORS da browser
            var httpResponse = HttpContext.Current.Response;
            httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            try {
                var response = _activityServices.TriggerSignal(signal.Name, signal.ContentId);
                return response;
            } catch (Exception ex) {
                return new Response { Success = false, Message = ex.Message, ErrorCode = ErrorCode.GenericError };
            }
        }

    }

    public class Signal {
        public string Name { get; set; }
        public int ContentId { get; set; }
    }
}