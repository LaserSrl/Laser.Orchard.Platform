using Laser.Orchard.Mobile.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class ContactListController : Controller {
        private readonly IPushGatewayService _pushService;

        public ContactListController(IPushGatewayService pushService) {
            _pushService = pushService;
        }

        [Authorize]
        [HttpPost]
        public JsonResult Search(string term) {
            List<FoundContact> result = new List<FoundContact>();
            ContactsArray array = new ContactsArray();
            var contatti = _pushService.GetContactsWithDevice(term);
            foreach (var contact in contatti) {
                result.Add(new FoundContact { 
                    label = String.Format(Convert.ToString(contact["Title"])+" ({0})", Convert.ToInt32(contact["NumDevice"])),
                    value = Convert.ToString(contact["Title"]).Trim()
                });
            }
            return Json(result.ToArray());
        }

        private class ContactsArray {
            public FoundContact[] elenco { get; set; }
        }

        private class FoundContact {
            public string value { get; set; }
            public string label { get; set; }
        }
    }
}