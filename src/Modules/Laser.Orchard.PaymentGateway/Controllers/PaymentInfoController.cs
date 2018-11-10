using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class PaymentInfoController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IPaymentService _paymentService;
        private readonly ISiteService _siteService;
        private dynamic Shape { get; set; }

        public PaymentInfoController(IOrchardServices orchardServices, IPaymentService paymentService, IShapeFactory shapeFactory, ISiteService siteService) {
            _orchardServices = orchardServices;
            _paymentService = paymentService;
            Shape = shapeFactory;
            _siteService = siteService;
        }
        /// <summary>
        /// Visualizza i pagamenti dell'utente corrente
        /// </summary>
        /// <returns></returns>
        [Themed]
        public ActionResult Index() {
            UserPayments model = new UserPayments();
            var user = _orchardServices.WorkContext.CurrentUser;
            int userId = -1; // valore che non corrisponde a nessun utente: se non sei autenticato non vedi nulla
            string userName = "";
            if (user != null) {
                userId = user.Id;
                userName = user.UserName;
            }
            model.Records = _paymentService.GetPayments(userId);
            model.UserName = userName;
            return View("Index", model);
        }
        /// <summary>
        /// Visualizza tutti i pagamenti.
        /// </summary>
        /// <param name="pagerParameters"></param>
        /// <returns></returns>
        [Admin]
        public ActionResult ListAll(PagerParameters pagerParameters) {
            if (_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner) == false) {
                return new HttpUnauthorizedResult();
            }
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(0);
            var list = Shape.List();
            List<PaymentRecord> pageOfContentItems = (List<PaymentRecord>)null;
            pageOfContentItems = _paymentService.GetAllPayments();
            pagerShape = Shape.Pager(pager).TotalItemCount(pageOfContentItems.Count);
            pageOfContentItems = pageOfContentItems.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();
            PaymentListVM model = new PaymentListVM {
                Records = pageOfContentItems,
                Pager = pagerShape
            };
            return View("All", model);
        }
    }
}