using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Themes;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class CustomPosController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly CustomPosService _posService;

        public CustomPosController(IOrchardServices orchardServices, CustomPosService posService) {
            _orchardServices = orchardServices;
            _posService = posService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [Themed]
        public ActionResult Index(int pid = 0, string guid = "") {
            PaymentRecord payment;

            if (pid > 0) {
                payment = _posService.GetPaymentInfo(pid);
            } else {
                payment = _posService.GetPaymentInfo(guid);
            }
            pid = payment.Id;

            // I check the payment as completed.
            _posService.EndPayment(pid, true, "", "", payment.Guid);

            PaymentVM model = new PaymentVM();
            model.Record = payment;

            return RedirectToAction("Info", "Payment", new { paymentId = payment.Id });
        }
    }
}