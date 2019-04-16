using Orchard.ContentManagement.Records;

namespace Laser.Orchard.ContactForm.Models
{
    /// <summary>
    /// The fields this content part requires.
    /// </summary>
    public class ContactFormRecord : ContentPartRecord 
    {
        public ContactFormRecord() {
            AcceptPolicy = false;
        }
        /// <summary>
        /// Gets or sets the recipient email address- the address the form will send an email to.
        /// </summary>
        public virtual string RecipientEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the static subject message- the subject of the email the recipient will receive.
        /// </summary>
        public virtual string StaticSubjectMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the static subject string, or provide an input on the form for it.
        /// </summary>
        public virtual bool UseStaticSubject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to provide an input on the form for a name field. This name will be included in the message.
        /// </summary>
        public virtual bool DisplayNameField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to require name field.
        /// </summary>
        public virtual bool RequireNameField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the id of the template to use. The default value -1 means no template.
        /// </summary>
        public virtual int TemplateRecord_Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable file upload in the form.
        /// </summary>
        public virtual bool EnableUpload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to attach or link the uploaded files in the emails.
        /// </summary>
        public virtual bool AttachFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the path where the uploaded files are saved.
        /// </summary>
        public virtual string PathUpload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to require the attachment.
        /// </summary>
        public virtual bool RequireAttachment { get; set; }
        public virtual bool AcceptPolicy { get; set; }
        public virtual string AcceptPolicyUrl { get; set; }
        public virtual string AcceptPolicyUrlText { get; set; }
        public virtual string AcceptPolicyText { get; set; }
        public virtual string ThankyouPage { get; set; }
    }
}