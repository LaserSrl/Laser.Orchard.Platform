using Orchard.ContentManagement;

namespace Laser.Orchard.ContactForm.Models {
    /// <summary>
    /// The content part model, uses the record class for storage.
    /// </summary>
    public class ContactFormPart : ContentPart<ContactFormRecord> {
        /// <summary>
        /// Gets or sets the recipient email address.
        /// </summary>
        public string RecipientEmailAddress {
            get { return this.Retrieve(x => x.RecipientEmailAddress); }
            set { this.Store(x => x.RecipientEmailAddress, value); }

        }

        /// <summary>
        /// Gets or sets the static subject message.
        /// </summary>
        public string StaticSubjectMessage {
            get { return this.Retrieve(x => x.StaticSubjectMessage); }
            set { this.Store(x => x.StaticSubjectMessage, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the static subject message.
        /// </summary>
        public bool UseStaticSubject {
            get { return this.Retrieve(x => x.UseStaticSubject); }
            set { this.Store(x => x.UseStaticSubject, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to display the name field.
        /// </summary>
        public bool DisplayNameField {
            get { return this.Retrieve(x => x.DisplayNameField); }
            set { this.Store(x => x.DisplayNameField, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to require the name field.
        /// </summary>
        public bool RequireNameField {
            get { return this.Retrieve(x => x.RequireNameField); }
            set { this.Store(x => x.RequireNameField, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating the id of the template to use. The default value -1 means no template.
        /// </summary>
        public int TemplateRecord_Id {
            get { return this.Retrieve(x => x.TemplateRecord_Id); }
            set { this.Store(x => x.TemplateRecord_Id, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable file upload in the form
        /// </summary>
        public bool EnableUpload {
            get { return this.Retrieve(x => x.EnableUpload); }
            set { this.Store(x => x.EnableUpload, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to attach or link the uploaded files in the emails
        /// </summary>
        public bool AttachFiles {
            get { return this.Retrieve(x => x.AttachFiles); }
            set { this.Store(x => x.AttachFiles, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating the path where the uploaded files are saved
        /// </summary>
        public string PathUpload {
            get { return this.Retrieve(x => x.PathUpload); }
            set { this.Store(x => x.PathUpload, value); }

        }

        /// <summary>
        /// Gets or sets a value indicating whether to require the attachment.
        /// </summary>
        public bool RequireAttachment {
            get { return this.Retrieve(x => x.RequireAttachment); }
            set { this.Store(x => x.RequireAttachment, value); }

        }
    }
}