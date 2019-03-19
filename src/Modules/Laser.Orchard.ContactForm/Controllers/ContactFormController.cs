using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.Services;
using Orchard;
using Orchard.Mvc.Extensions;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.ContactForm.Controllers
{
    /// <summary>
    /// The controller that handles all contact form requests.
    /// </summary>
    public class ContactFormController : Controller
    {
        private readonly IContactFormService _contactFormService;
        private readonly IOrchardServices _orchardServices;

        public ContactFormController(IContactFormService contactFormService, IOrchardServices orchardServices)
        {
            _contactFormService = contactFormService;
            _orchardServices = orchardServices;
        }

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
        
        public ActionResult SendContactEmail(int id, string returnUrl, string name, string email, string confirmEmail, string subject, string message, int mediaid = -1, int Accept = 0)
        {
            try {
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
                }
            } catch 
            {
                // L'eccezione serve solo per la chiamata via APIController, mentre per la chiamata via form è già stata loggata e salvata nel Notifier
                TempData["form"] = Request.Form;
            }

            return this.RedirectLocal(returnUrl, "~/");
        }
    }
}