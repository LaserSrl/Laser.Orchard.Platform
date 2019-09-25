using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace Laser.Orchard.Mobile.Controllers {
    public class CountPanelController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly ISmsCommunicationService _smsCommunicationService;
        public CountPanelController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _orchardServices.WorkContext.TryResolve<IPushGatewayService>(out _pushGatewayService);
            _orchardServices.WorkContext.TryResolve<ISmsCommunicationService>(out _smsCommunicationService);
        }

        [HttpPost]
        [AdminService]
        public JsonResult GetTotalPush(Int32[] ids, string[] manualRecipients, int? contentId, Int32? idlocalization, Int32? tot) {
            if (manualRecipients != null) {
                manualRecipients = manualRecipients.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-mobile\"></i>");
            if (tot.HasValue) {
                Total.Add("Value", tot.ToString());
            } else {
                IList elenco;
                if (manualRecipients == null || manualRecipients.Length == 0) {
                    elenco = _pushGatewayService.GetPushQueryResult(ids, true, contentId.HasValue ? contentId.Value : 0);
                } else {
                    elenco = _pushGatewayService.CountPushQueryResultByUserNames(manualRecipients, null, true, "All");
                }
                var android = Convert.ToInt64((((Hashtable)(elenco[0]))["Android"]) ?? 0); //elenco.Where(x => x.Device == TipoDispositivo.Android).Count();
                var apple = Convert.ToInt64((((Hashtable)(elenco[0]))["Apple"]) ?? 0);  //elenco.Where(x => x.Device == TipoDispositivo.Apple).Count();
                var win = Convert.ToInt64((((Hashtable)(elenco[0]))["WindowsMobile"]) ?? 0);  //elenco.Where(x => x.Device == TipoDispositivo.WindowsMobile).Count();
                Total.Add("Value", string.Format("{0:#,##0} (<i class=\"fa fa-android\"></i> {1:#,##0}, <i class=\"fa fa-apple\"></i> {2:#,##0}, <i class=\"fa fa-windows\"></i> {3:#,##0})", ((long)(((Hashtable)(elenco[0]))["Tot"])), android, apple, win));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AdminService]
        public JsonResult GetTotalSms(Int32[] ids, string[] manualRecipients, int? contentId, Int32? idlocalization, Int32? tot) {
            if (manualRecipients != null) {
                manualRecipients = manualRecipients.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-phone\"></i>");
            if (tot.HasValue) {
                Total.Add("Value", tot.ToString());
            } else {
                if (manualRecipients == null || manualRecipients.Length == 0) {
                    var elenco = _smsCommunicationService.GetSmsQueryResult(ids, idlocalization, true, contentId.HasValue ? contentId.Value : 0);
                    Total.Add("Value", ((long)(((Hashtable)(elenco[0]))["Tot"])).ToString("#,##0"));
                } else {
                    Total.Add("Value", manualRecipients.Length.ToString("#,##0"));
                }
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}