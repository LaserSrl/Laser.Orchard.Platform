using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.Services;
using Laser.Orchard.ContactForm.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Localization;
using System;
using System.Web.Http;


namespace Laser.Orchard.ContactForm.Controllers
{
    [WebApiKeyFilter(false)]
    public class EmailAPIController : ApiController
    {

        private readonly IContactFormService _contactFormService;
        public Localizer T { get; set; }

        public EmailAPIController(IContactFormService contactFormService)
        {
            _contactFormService = contactFormService;
            T = NullLocalizer.Instance;
        }

        public void Get()
        { }

        [System.Web.Mvc.HttpPost]
        public EmailAPIResultModel Post([FromBody] EmailAPIModel emailAPIModel)
        {
            int mediaId = -1;
            string errorString = "";

            try
            {
                if (emailAPIModel == null)
                    errorString = T("The provided data does not correspond to the required format.").ToString();
                else
                {
                    ContactFormRecord contactForm = _contactFormService.GetContactForm(emailAPIModel.ContentId);

                    if (contactForm == null)
                        errorString = T("The content Id has not been provided or does not correspond to a content part of the correct type.").ToString();

                    if (errorString == "")
                        errorString = _contactFormService.ValidateAPIRequest(emailAPIModel.SenderName, emailAPIModel.SendFrom, emailAPIModel.MessageText, contactForm.RequireNameField, (emailAPIModel.Attachment.Length > 0), contactForm.RequireAttachment);

                    if (errorString == "" && !string.IsNullOrWhiteSpace(emailAPIModel.Attachment) && !string.IsNullOrWhiteSpace(emailAPIModel.AttachmentName))
                    {
                        if (_contactFormService.FileAllowed(emailAPIModel.AttachmentName))
                        {
                            mediaId = _contactFormService.UploadFromBase64(emailAPIModel.Attachment, contactForm.PathUpload, emailAPIModel.AttachmentName);
                        }
                        else
                        {
                            errorString = T("The file extension in the filename is not allowed or has not been provided.").ToString();
                        }
                    }

                    if (errorString == "")
                    {
                        _contactFormService.SendContactEmail(emailAPIModel.SenderName, emailAPIModel.SendFrom, emailAPIModel.SendFrom, emailAPIModel.MessageSubject, emailAPIModel.MessageText, mediaId, contactForm, emailAPIModel.AdditionalData);
                    }
                }
            }
            catch (Exception e)
            {
                errorString = e.Message;
            }

            return new EmailAPIResultModel { Error = errorString, Information = "" };
        }

        public void Put()
        { }

        public void Delete()
        { }

    }
}
