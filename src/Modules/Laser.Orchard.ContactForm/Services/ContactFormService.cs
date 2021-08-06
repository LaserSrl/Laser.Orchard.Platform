using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Email.Models;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Messaging.Services;
using Orchard.UI.Notify;
using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.Email.Services;

namespace Laser.Orchard.ContactForm.Services {
    public class ContactFormService : IContactFormService {

        private readonly IOrchardServices _orchardServices;
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly INotifier _notifier;
        private readonly IRepository<ContactFormRecord> _contactFormRepository;
        private readonly ITemplateService _templateServices;
        private readonly IContentManager _contentManager;
        private readonly ISmtpChannel _messageManager;
        private readonly IStorageProvider _storageProvider;

        private string validationError = "";

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ContactFormService(IOrchardServices orchardServices, INotifier notifier, IRepository<ContactFormRecord> contactFormRepository, ITemplateService templateServices, IContentManager contentManager, ISmtpChannel messageManager, IMediaLibraryService mediaLibraryService, IStorageProvider storageProvider) {
            _notifier = notifier;
            _orchardServices = orchardServices;
            _contactFormRepository = contactFormRepository;
            _templateServices = templateServices;
            _messageManager = messageManager;
            _contentManager = contentManager;
            _mediaLibraryService = mediaLibraryService;
            _storageProvider = storageProvider;
            Logger = NullLogger.Instance;
        }

        #region Implementation of IContactFormService

        /// <summary>
        /// Gets the contact form record.
        /// </summary>
        /// <param name="id">The record id.</param>
        public ContactFormRecord GetContactForm(int id) {
            return _contactFormRepository.Get(id);
        }

        /// <summary>
        /// Sends a contact email.
        /// </summary>
        /// <param name="name">The name of the sender</param>
        /// <param name="confirmEmail">The email address of the sender.</param>
        /// <param name="email">The email address entered in by spam bot</param>
        /// <param name="subject">The email subject</param>
        /// <param name="message">The email message</param>
        /// <param name="mediaid">The id of the attached file or -1 if no file is provided</param>
        /// <param name="record">The content of the Contact Form</param>
        public void SendContactEmail(string name, string confirmEmail, string email, string subject, string message, int mediaid, ContactFormRecord record, object additionalData = null)
        {
            SendContactEmail(name, confirmEmail, email, subject, message, mediaid, record.RecipientEmailAddress, record.RequireNameField, record.RequireAttachment, record.UseStaticSubject, record.TemplateRecord_Id, record.AttachFiles, additionalData);
        }

        /// <summary>
        /// Sends a contact email.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="confirmEmail">The email address of the sender.</param>
        /// <param name="email">The email address entered in by spam bot</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="message">The email message.</param>
        /// <param name="mediaid">The id of the attached file or -1 if no file is provided</param>
        /// <param name="sendTo">The email address to send the message to.</param>
        /// <param name="requiredName">Bool of Name is required</param>
        /// <param name="useStaticSubject">Boolean indicating if a fixed subject must be used for all emails</param>
        /// <param name="templateId">The id of the mail template or -1 if no template is selected</param>
        /// <param name="attachFiles">Boolean indicating whether to attach uploaded files (true) or only add their URL to the body of the mail (false)</param>
        public void SendContactEmail(string name, string confirmEmail, string email, string subject, string message, int mediaid, string sendTo, bool requiredName, bool requiredAttachment = false, bool useStaticSubject = false, int templateId = -1, bool attachFiles = false, object additionalData = null) {

            if (ValidateContactFields(name, email, message, requiredName, (mediaid != -1), requiredAttachment)) {
                if (string.IsNullOrEmpty(name))
                    name = email;

                var host = string.Format("{0}://{1}{2}",
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);

                var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
                var smtpLogger = new SmtpLogger();
                ((Component)_messageManager).Logger = smtpLogger;

                if (smtpSettings != null && smtpSettings.IsValid()) {
                    var mailClient = BuildSmtpClient(smtpSettings);
                    var contactFormSettings = _orchardServices.WorkContext.CurrentSite.As<ContactFormSettingsPart>().Record;

                    if (confirmEmail != email) {
                        // This option allows spam to be sent to a separate email address.
                        if (contactFormSettings.EnableSpamEmail && !string.IsNullOrEmpty(contactFormSettings.SpamEmail)) {
                            try {

                                MediaPart mediaData = new MediaPart();
                                if (mediaid != -1) {
                                    mediaData = _contentManager.Get(mediaid).As<MediaPart>();
                                }

                                var body = "<strong>" + name + "</strong><hr/>" + "<br/><br/><div>" + message + "</div>";
                                if (mediaid != -1 && !attachFiles) {
                                    body += "<div><a href=\"" + host + mediaData.MediaUrl + "\">" + T("Attachment") + "</a></div>";
                                }
                                var data = new Dictionary<string, object>();
                                data.Add("Recipients", sendTo);
                                data.Add("ReplyTo", email);
                                data.Add("Subject", subject);
                                data.Add("Body", body);
                                if (mediaid != -1 && attachFiles) {
                                    data.Add("Attachments", new string[] { _orchardServices.WorkContext.HttpContext.Server.MapPath(mediaData.MediaUrl) });
                                }

                                _messageManager.Process(data);

                                string spamMessage = string.Format(T("Your message was flagged as spam. If you feel this was in error contact us directly at: {0}").Text, sendTo);
                                _notifier.Information(T(spamMessage));
                            } catch (Exception e) {
                                Logger.Error(e, T("An unexpected error while sending a contact form message flagged as spam to {0} at {1}").Text, contactFormSettings.SpamEmail, DateTime.Now.ToLongDateString());
                                string errorMessage = string.Format(T("Your message was flagged as spam. If you feel this was in error contact us directly at: {0}").Text, sendTo);
                                _notifier.Error(T(errorMessage));
                            }
                        } else {
                            _notifier.Error(T("'Confirm email' and 'Email' does not match. Please retry."));
                        }
                    } else {

                        try {

                            if (templateId > 0) {

                                ParseTemplateContext templatectx = new ParseTemplateContext();
                                var template = _templateServices.GetTemplate(templateId);
                                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

                                string mailSubject = "";
                                if (useStaticSubject) /*Contatct form has been set to use a static message (in the front-end form the subject field is not displayed)*/{
                                    if (String.IsNullOrWhiteSpace(subject) /*the ContactForm subject is empty*/
                                    && !String.IsNullOrWhiteSpace(template.Subject)) /*the template subject is not empty*/ {
                                        mailSubject = template.Subject;
                                    }
                                    else {
                                        mailSubject = subject; //subject is set via the back-end part
                                    }
                                }
                                else {
                                    mailSubject = subject; //subject is set via the front-end textbox
                                }

                                MediaPart mediaData = new MediaPart();
                                if (mediaid != -1) {
                                    mediaData = _contentManager.Get(mediaid).As<MediaPart>();
                                }

                                dynamic model = new {
                                    SenderName = name,
                                    SenderEmail = email,
                                    Subject = mailSubject,
                                    Message = message,
                                    AttachmentUrl = (mediaid != -1 ? host + mediaData.MediaUrl : ""),
                                    AdditionalData = additionalData
                                };

                                // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
                                // Nel template pertanto Model, diventa Model.Content
                                templatectx.Model = model;
                                var body = _templateServices.ParseTemplate(template, templatectx);
                                var data = new Dictionary<string, object>();
                                data.Add("Recipients", sendTo);
                                data.Add("ReplyTo", email);
                                data.Add("Subject", mailSubject);
                                data.Add("Body", body);
                                if (mediaid != -1 && attachFiles) {
                                    data.Add("Attachments", new string[] { _orchardServices.WorkContext.HttpContext.Server.MapPath(mediaData.MediaUrl) });
                                }

                                _messageManager.Process(data);

                            } else {

                                MediaPart mediaData = new MediaPart();
                                if (mediaid != -1) {
                                    mediaData = _contentManager.Get(mediaid).As<MediaPart>();
                                }

                                var body = "<strong>" + name + "</strong><hr/>" + "<br/><br/><div>" + message + "</div>";
                                if (mediaid != -1 && !attachFiles) {
                                    body += "<div><a href=\"" + host + mediaData.MediaUrl + "\">" + T("Attachment") + "</a></div>";
                                }
                                var data = new Dictionary<string, object>();
                                data.Add("Recipients", sendTo);
                                data.Add("ReplyTo", email);
                                data.Add("Subject", subject);
                                data.Add("Body", body);
                                if (mediaid != -1 && attachFiles) {
                                    data.Add("Attachments", new string[] { _orchardServices.WorkContext.HttpContext.Server.MapPath(mediaData.MediaUrl) });
                                }

                                _messageManager.Process(data);
                            }
                            if (!string.IsNullOrWhiteSpace(smtpLogger.ErrorMessage)) {
                                throw new Exception(smtpLogger.ErrorMessage, smtpLogger.Exception);
                            }

                            Logger.Debug(T("Contact form message sent to {0} at {1}").Text, sendTo, DateTime.Now.ToLongDateString());
                            _notifier.Information(T("Operation completed successfully."));
                            //_notifier.Information(T("Thank you for your inquiry, we will respond to you shortly."));
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, T("An unexpected error while sending a contact form message to {0} at {1}").Text, sendTo, DateTime.Now.ToLongDateString());
                            var errorMessage = string.Format(T("An unexpected error occured when sending your message. You may email us directly at: {0}").Text, sendTo);
                            _notifier.Error(T(errorMessage));
                            throw new System.Exception(errorMessage);
                        }
                    }
                } else {
                    string errorMessage = string.Format(T("Our email server isn't configured. You may email us directly at: {0}").Text, sendTo);
                    _notifier.Error(T(errorMessage));
                    throw new System.Exception(errorMessage);
                }
            } else {
                _notifier.Error(T(validationError));
                throw new System.Exception(T(validationError).Text);
            }
        }

        public int UploadFromBase64(string file, string uploadFolder, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(file)) {
                byte[] fileBytes = Convert.FromBase64String(file);
                MemoryStream stream = new MemoryStream(fileBytes);

                var mediaPart = _mediaLibraryService.ImportMedia(stream, uploadFolder, fileName);
                _contentManager.Create(mediaPart);
                return mediaPart.Id;
            } else {
                return -1;
            }
        }

        public bool FileAllowed(string fileName)
        {
            return FileExtensionAllowed(Path.GetExtension(fileName).Trim().TrimStart('.'));
        }

        public bool FileExtensionAllowed(string fileExtension)
        {
            //TODO: Permettere di definire la lista di estensioni come setting
            string[] allowedFiles = new string[] {"jpg", "png", "doc", "docx", "xls", "xlsx", "pdf"};
            return allowedFiles.Contains(fileExtension.ToLowerInvariant());
        }

        #endregion

        private SmtpClient BuildSmtpClient(SmtpSettingsPart smtpSettings) {
            var mailClient = new SmtpClient {
                Host = smtpSettings.Host,
                Port = smtpSettings.Port,
                EnableSsl = smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = !smtpSettings.RequireCredentials
            };
            if (!mailClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.UserName)) {
                mailClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
            }
            return mailClient;
        }

        private bool ValidateContactFields(string name, string email, string message, bool nameRequired, bool hasAttachment, bool attachmentRequired) {
            bool isValid = true;
            const string emailAddressRegex = @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$";

            if ((nameRequired && String.IsNullOrEmpty(name)) || (attachmentRequired && !hasAttachment) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message)) {
                //_notifier.Error(T("All contact fields are required."));
                if (nameRequired && attachmentRequired) {
                    validationError = "The sender email, sender name, message text and attachment are required, but one or more of these informations are missing.";
                } else if (nameRequired) {
                    validationError = "The sender email, sender name and message text are required, but one or more of these informations are missing.";
                } else if (attachmentRequired) {
                    validationError = "The sender email, message text and attachment are required, but one or more of these informations are missing.";
                } else {
                    validationError = "The sender email and the message text are required, but one or more of these informations are missing.";
                }
                isValid = false;
            } else {
                Match emailMatch = Regex.Match(email, emailAddressRegex);
                if (!emailMatch.Success) {
                    //_notifier.Error(T("Invalid email address."));
                    validationError = "Invalid email address.";
                    isValid = false;
                }
            }

            return isValid;
        }

        public string ValidateAPIRequest(string name, string email, string message, bool nameRequired, bool hasAttachment, bool attachmentRequired)
        {
            ValidateContactFields(name, email, message, nameRequired, hasAttachment, attachmentRequired);
            return validationError;
        }

        private class SmtpLogger : ILogger {
            public SmtpLogger() {
            }

            public string ErrorMessage { get; set; }
            public Exception Exception { get; set; }
            public bool IsEnabled(LogLevel level) {
                return true;
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args) {
                if (level == LogLevel.Error || level == LogLevel.Fatal) { // populate message only if the Level is Error
                    ErrorMessage = exception == null ? format : exception.Message;
                    Exception = exception;
                }
            }
        }

    }

    
}