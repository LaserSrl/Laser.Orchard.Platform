using Laser.Orchard.Braintree.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Braintree.Controllers
{
    public class TestController : Controller
    {
        private readonly IOrchardServices _orchardServices;

        public TestController(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
        }

        public ActionResult Index()
        {
            var config = _orchardServices.WorkContext.CurrentSite.As<BraintreeSiteSettingsPart>();
            if (config.ProductionEnvironment)
            {
                // ridireziona su una action inesistente perevitare di far spendere oldi veri per una transazione di test
                return RedirectToAction("NotAvailable");
            }
            return View();
        }
    }
}