using Orchard;
using Laser.Orchard.ContactForm.Models;

namespace Laser.Orchard.ContactForm.Services
{
    public interface IContactFormService : IDependency
    {
        /// <summary>
        /// Gets the contact form record.
        /// </summary>
        /// <param name="id">The record id.</param>
        ContactFormRecord GetContactForm(int id);

        /// <summary>
        /// Sends a contact email.
        /// </summary>
        /// <param name="name">The name of the sender</param>
        /// <param name="confirmEmail">The email address of the sender.</param>
        /// <param name="email">The email address entered in by spam bot</param>
        /// <param name="subject">The email subject</param>
        /// <param name="message">The email message</param>
        /// <param name="mediaid">The id of the attached file or -1 if no file is provided</param>
        /// <param name="sendTo">The email address to send the message to.</param>
        /// <param name="requiredName">Boolean indicating if Name is required</param>
        /// <param name="useStaticSubject">Boolean indicating if a fixed subject must be used for all emails</param>
        /// <param name="templateId">The id of the mail template or -1 if no template is selected</param>
        /// <param name="attachFiles">Boolean indicating whether to attach uploaded files (true) or only add their URL to the body of the mail (false)</param>
        /// <param name="additionalData">A collection of values that is passed to the template (i.e. a FormCollection or a Dictionary)</param>
        void SendContactEmail(string name, string confirmEmail, string email, string subject, string message, int mediaid, string sendTo, bool requiredName, bool requiredAttachment = false, bool useStaticSubject = false, int templateId = -1, bool attachFiles = false, object additionalData = null);

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
        /// <param name="additionalData">A collection of values that is passed to the template (i.e. a FormCollection or a Dictionary)</param>
        void SendContactEmail(string name, string confirmEmail, string email, string subject, string message, int mediaid, ContactFormRecord record, object additionalData = null);

        /// <summary>
        /// Uploads a file from the provided path
        /// </summary>
        /// <param name="file">The file encoded as a base64 string</param>
        /// <param name="uploadFolder">The folder where the file must be uploaded</param>
        /// <returns>The mediaID of the uploaded file</returns>
        int UploadFromBase64(string file, string uploadFolder, string fileName);

        /// <summary>
        /// Checks if the extension of the file is allowed for upload
        /// </summary>
        /// <param name="fileExtension">The file extension</param>
        /// <returns>True if the extension is allowed, false otherwise</returns>
        bool FileExtensionAllowed(string fileExtension);

        /// <summary>
        /// Checks if the extension of the file is allowed for upload
        /// </summary>
        /// <param name="fileExtension">The complete file name with its extension</param>
        /// <returns>True if the extension is allowed, false otherwise</returns>
        bool FileAllowed(string fileName);

        /// <summary>
        /// Checks if the API request contains all required data
        /// </summary>
        /// <param name="name">The name of the sender</param>
        /// <param name="email">The email of the sender</param>
        /// <param name="message">The email message</param>
        /// <param name="nameRequired">A flag indicating if the name is required</param>
        /// <param name="attachmentLength">The length of the attachment or -1 if there is no attachment</param>
        /// <param name="attachmentRequired">A flag indicating if the attachment is required</param>
        /// <returns></returns>
        string ValidateAPIRequest(string name, string email, string message, bool nameRequired, bool hasAttachment, bool attachmentRequired);
    }
}
