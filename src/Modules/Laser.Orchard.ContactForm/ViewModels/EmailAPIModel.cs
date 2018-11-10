using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.ContactForm.ViewModels {

    public class EmailAPIModel
    {
        /// <summary>
        /// The ID of the part containing the email settings (template, attachment options, upload path, etc.)
        /// </summary>
        public int ContentId { get; set; }

        /// <summary>
        /// The name of the sender
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// The mail address of the sender
        /// </summary>
        public string SendFrom { get; set; }

        /// <summary>
        /// The subject of the email
        /// </summary>
        public string MessageSubject { get; set; }

        /// <summary>
        /// The body of the email
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Additional data to pass to the template
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }

        /// <summary>
        /// The name of the attachment including its extension
        /// </summary>
        public string AttachmentName { get; set; }

        /// <summary>
        /// The attachment as a Base64 string
        /// </summary>
        public string Attachment { get; set; }

    }
}