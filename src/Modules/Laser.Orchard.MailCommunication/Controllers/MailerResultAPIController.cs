using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.MailCommunication.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MailCommunication.Controllers
{
    // questa API utilizza un proprio token per la protezione
    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailerResultAPIController : ApiController
    {
        public class DatiPost
        {
            public int num { get; set; }
        }
        private readonly IUtilsServices _utilsServices;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public MailerResultAPIController(IOrchardServices orchardServices, IUtilsServices utilsServices, IContentManager contentManager)
        {
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
            _contentManager = contentManager;
        }

        public System.Web.Http.Results.JsonResult<Laser.Orchard.StartupConfig.ViewModels.Response> Post([FromUri] string tk, [FromBody] DatiPost valori)
        {
            Response result = _utilsServices.GetResponse(ResponseType.Validation, "Validation error.");
            try
            {
                int id = 0;
                var tkOrig = System.Text.ASCIIEncoding.Unicode.GetString(Convert.FromBase64String(tk));
                if (tkOrig.Length > 10)
                {
                    var mailerConfig = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
                    DateTime dt0 = DateTime.ParseExact(tkOrig.Substring(0, 10), "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
                    if (DateTime.Now < dt0.AddDays(mailerConfig.TokenValidity))
                    {
                        id = int.Parse(tkOrig.Substring(10));

                        // aggiorna il dato su orchard
                        var item = _contentManager.Get<MailCommunicationPart>(id);
                        item.SentMailsNumber += valori.num;

                        result = _utilsServices.GetResponse(ResponseType.Success, "OK");
                    }
                }
            }
            catch
            {
                // non fa nulla perché la risposta negativa è già il default e non si voglionon dare dettagli sul'errore a eventuali hacker
            }
            return Json(result);
        }
	}
}