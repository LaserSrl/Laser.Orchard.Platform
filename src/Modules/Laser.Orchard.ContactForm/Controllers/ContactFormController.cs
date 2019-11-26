using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.ContactForm.Controllers {
    /// <summary>
    /// The controller that handles all contact form requests.
    /// </summary>
    public class ContactFormController : Controller, IUpdateModel {
        private readonly IContactFormService _contactFormService;
        private readonly IOrchardServices _orchardServices;
        private readonly IFrontEndEditService _frontEndEditeService;
        private readonly IContentManager _contentManager;

        public ContactFormController(
            IContactFormService contactFormService,
            IOrchardServices orchardServices,
            IFrontEndEditService frontEndEditService,
            IContentManager contentManager) {

            _contactFormService = contactFormService;
            _orchardServices = orchardServices;
            _frontEndEditeService = frontEndEditService;
            _contentManager = contentManager;
        }

        Func<ContentTypePartDefinition, string, bool> OnlyShowReCaptcha =
            (ctpd, typeName) =>
                ctpd.PartDefinition.Name == "ReCaptchaPart";
        Func<ContentPartFieldDefinition, bool> NoFields =
            (ctpd) =>
                false;

        /// <summary>
        /// Sends the contact email.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="name">The name.</param>
        /// <param name="email">The bot false email.</param>
        /// <param name="confirmEmail">The actual email string</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>

        public ActionResult SendContactEmail(int id, string returnUrl, string name, string email, string confirmEmail, string subject, string message, int mediaid = -1, int Accept = 0) {
            var redirectionUrl = returnUrl;
            try {

                // we want to create a new contentItem of the same type as the form we are posting
                var stubItem = _contentManager.Get<ContactFormPart>(id);
                // then we will try to launch UPdateEditor to test recaptcha.
                if (stubItem != null) {
                    _frontEndEditeService.BuildFrontEndShape(
                        _contentManager.UpdateEditor(stubItem, this),
                        OnlyShowReCaptcha,
                        NoFields);
                    if (!ModelState.IsValid) {
                        // consider logging
                        // update of recaptcha failed
                        _orchardServices.TransactionManager.Cancel();
                        TempData["form"] = Request.Form;
                        return this.RedirectLocal(redirectionUrl, "~/");
                    }
                }
                ContactFormRecord contactForm = _contactFormService.GetContactForm(id);
                if (contactForm.AcceptPolicy && Accept != 1) {
                    TempData["form"] = Request.Form;
                    return this.RedirectLocal(Request.UrlReferrer.ToString());
                }
                if (contactForm != null) {
                    // If a static subject message was specified, use that value for the email subject.
                    if (contactForm.UseStaticSubject) {
                        if (contactForm.StaticSubjectMessage != null)
                            subject = contactForm.StaticSubjectMessage.Replace("{NAME}", name);
                        if (Request.Url != null)
                            subject = subject.Replace("{DOMAIN}", Request.Url.Host);
                    }

                    _contactFormService.SendContactEmail(name, confirmEmail, email, subject, message, mediaid, contactForm, _orchardServices.WorkContext.HttpContext.Request.Form);
                    if (!string.IsNullOrWhiteSpace(contactForm.ThankyouPage)) {
                        redirectionUrl = contactForm.ThankyouPage;
                    }
                }
            } catch {
                // L'eccezione serve solo per la chiamata via APIController, mentre per la chiamata via form è già stata loggata e salvata nel Notifier
                TempData["form"] = Request.Form;
                redirectionUrl = returnUrl;
            }

            return this.RedirectLocal(redirectionUrl, "~/");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}