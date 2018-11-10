using Laser.Orchard.MailCommunication.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Controllers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class UnsubscribeController : Controller {

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        private readonly INotifier _notifier;
        private readonly IMailUnsubscribeService _service;

        public UnsubscribeController(INotifier notifier, IShapeFactory shapeFactory, IMailUnsubscribeService service) {
            _notifier = notifier;
            _service = service;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        [Themed, HttpGet]
        public ActionResult Index() {
            return new ShapeResult(this, Shape.Unsubscribe_Confirm());
        }

        [Themed, HttpPost]
        public ActionResult UnsubscribeMail(string email, string confirmEmail) {
            bool allOK = false;

            if (HttpContext.Request.Form["Mail_Unsubscribe_Confirm"] == "") {
                return null;
            } else {
                if (email == confirmEmail && email.Trim() != "") {

                    try {
                        allOK = _service.SendMailConfirmUnsubscribe(email);
                    } 
                    catch (Exception ex) {
                        ModelState.AddModelError("UnsubscribeMailError", ex);
                    }

                } else {
                    if (email == "" || confirmEmail == "") {
                        _notifier.Error(T("Please insert mail and confirm mail"));
                    } else if (email != confirmEmail) {
                        _notifier.Error(T("Email and email confirmation must match!"));
                    }
                }
            }

            if (allOK) {
                return new ShapeResult(this, Shape.Unsubscribe_EmailSent());
            } else {
                return new ShapeResult(this, Shape.Unsubscribe_Error());
            } 
        }

        [Themed]
        public ActionResult ConfirmUnsubscribe(string key) {
            bool allOK = false;

            try {
                allOK = _service.UnsubscribeMail(key);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("UnsubscribeMailError", ex);
            }

            if (allOK) {
                _notifier.Information(T("Unsubscription success"));
                return new ShapeResult(this, Shape.Unsubscribe_EmailSuccess());
            } else {
                return new ShapeResult(this, Shape.Unsubscribe_Error());
            }
        }

    }
}