using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Linq;
using System.Web.Mvc;


namespace Laser.Orchard.Mobile.Controllers {
    public class PushNotificationController : Controller {

        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;

        public PushNotificationController(
            IOrchardServices orchardServices
            , IPushNotificationService pushNotificationService
            , INotifier notifier
            ) {
            _orchardServices = orchardServices;
            _pushNotificationService = pushNotificationService;
            _notifier = notifier;
        }

       

        [System.Web.Mvc.HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, PushSearch search) {
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, PushSearch search) {
            if(search.Operation == "Rename") {
                _pushNotificationService.ReassignDevices(search.SelectedMachineName);
            }
            else if(search.Operation == "Clear") {
                search.Expression = "";
            }
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var tuple = _pushNotificationService.SearchPushNotification(search.Expression, pager.GetStartIndex(), pager.PageSize);
            var AllRecord = tuple.Item1;
            var totRecord = tuple.Item2;
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totRecord);
            var model = new PushIndex(AllRecord, search, pagerShape);
            return View(model);
        }

        public void Crea() {
            PushNotificationRecord test = new PushNotificationRecord();
            test.DataInserimento = DateTime.Today;
            test.DataModifica = DateTime.Today;
            test.Device = TipoDispositivo.Apple;
            //test.Id = 0;
            test.Validated = true;
            test.Language = "ITA";
            test.Produzione = true;
            test.Token = "awerwqdasfafsa";
            test.UUIdentifier = "iosonounid";
            _pushNotificationService.StorePushNotification(test);
            
        }

        //[Admin]
        //public ActionResult Send() {
        //    _PushNotificationService.SendPush(1, "Prova con Orchard");
        //    return View();
        //}

    }
}