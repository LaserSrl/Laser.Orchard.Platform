using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;

        public ContactFormController(
            IContactFormService contactFormService,
            IOrchardServices orchardServices,
            IFrontEndEditService frontEndEditService,
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            INotifier notifier) {

            _contactFormService = contactFormService;
            _orchardServices = orchardServices;
            _frontEndEditeService = frontEndEditService;
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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

        public ActionResult SendContactEmail(int id, string returnUrl, string name, string email, string confirmEmail, string subject, string message, int mediaId = -1, int accept = 0) {
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
                if (contactForm.AcceptPolicy && accept != 1) {
                    TempData["form"] = Request.Form;
                    return this.RedirectLocal(Request.UrlReferrer.ToString());
                }

                #region Validation Field
                bool isValid = true;
                const string emailAddressRegex = @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$";

                if ((contactForm.RequireNameField && String.IsNullOrEmpty(name)) ||
                    (contactForm.RequireAttachment && !(mediaId != -1)) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(confirmEmail)) {

                    if (string.IsNullOrEmpty(email)) {
                        ModelState.AddModelError("email",T("The email is mandatory.").Text);
                    }
                    if (string.IsNullOrEmpty(confirmEmail)) {
                        ModelState.AddModelError("confirmEmail", T("The confirm email is mandatory.").Text);
                    }
                    if (contactForm.RequireNameField && String.IsNullOrEmpty(name)) {
                        ModelState.AddModelError("name", T("The sender name is mandatory.").Text);
                    }
                    if (contactForm.RequireAttachment && !(mediaId != -1)) {
                        ModelState.AddModelError("mediaId", T("The attachment is mandatory.").Text);
                    }
                    if (string.IsNullOrEmpty(message)) {
                        ModelState.AddModelError("message", T("The message text is mandatory.").Text);
                    }
                    isValid = false;
                }
                else {
                    Match emailMatch = Regex.Match(email, emailAddressRegex);
                    Match confirmEmailMatch = Regex.Match(confirmEmail, emailAddressRegex);
                    if (!emailMatch.Success) {
                        ModelState.AddModelError("email", T("Invalid email address.").Text);
                        isValid = false;
                    }
                    if (!confirmEmailMatch.Success) {
                        ModelState.AddModelError("confirmEmail", T("Invalid confirm email address.").Text);
                        isValid = false;
                    }
                    if (isValid && email != confirmEmail) {
                        ModelState.AddModelError("confirmEmail", T("Confirm email must be matching to the email.").Text);
                        isValid = false;
                    }
                }
                if (!isValid) {
                    TempData["form"] = Request.Form;
                    _notifier.Add(NotifyType.Error, MessageError(ModelState));
                    return this.RedirectLocal(redirectionUrl, "~/");
                }
                #endregion

                #region TriggerValidation
                _workflowManager.TriggerEvent("ContactFormValidating",
                                                  stubItem,
                                                  () => new Dictionary<string, object> {
                                                    {"Content", stubItem},
                                                    {"ModelState", ModelState },
                                                    {"FormData",Request.Form },
                                                    {"Updater", this },
                                                    {"T", T }
                                                  });

                if (!ModelState.IsValid) {
                    TempData["form"] = Request.Form;
                     _notifier.Add(NotifyType.Error, MessageError(ModelState));
                    return this.RedirectLocal(redirectionUrl, "~/");
                }
                #endregion

                if (contactForm != null) {
                    // If a static subject message was specified, use that value for the email subject.
                    if (contactForm.UseStaticSubject) {
                        if (contactForm.StaticSubjectMessage != null)
                            subject = contactForm.StaticSubjectMessage.Replace("{NAME}", name);
                        if (Request.Url != null)
                            subject = subject.Replace("{DOMAIN}", Request.Url.Host);
                    }

                    _contactFormService.SendContactEmail(name, confirmEmail, email, subject, message, mediaId, contactForm, _orchardServices.WorkContext.HttpContext.Request.Form);
                    // after sending email it triggers a worflow event in order to execute arbitrary code.
                    _workflowManager.TriggerEvent("ContactFormSubmittedEvent",
                                                  stubItem,
                                                  () => new Dictionary<string, object> {
                                                    {"Content", stubItem}
                                                  });
                    if (!string.IsNullOrWhiteSpace(contactForm.ThankyouPage)) {
                        redirectionUrl = contactForm.ThankyouPage;
                    }
                }
            }
            catch {
                // L'eccezione serve solo per la chiamata via APIController, mentre per la chiamata via form è già stata loggata e salvata nel Notifier
                TempData["form"] = Request.Form;
                redirectionUrl = returnUrl;
            }

            return this.RedirectLocal(redirectionUrl, "~/");
        }


        private LocalizedString MessageError(ModelStateDictionary modelState) {
            StringBuilder sbError = new StringBuilder();
            foreach (string key in ModelState.Keys) {
                if (ModelState[key].Errors.Count > 0)
                    foreach (var error in ModelState[key].Errors) {
                        sbError.AppendLine(error.ErrorMessage);
                    }
            }
            sbError.AppendLine(T("Please correct the errors and try again.").Text);

            return T(sbError.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}