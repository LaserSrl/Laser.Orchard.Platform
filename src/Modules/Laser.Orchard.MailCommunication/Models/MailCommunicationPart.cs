using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MailCommunication.Models {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationPart : ContentPart {
        public bool MailMessageSent {
            get { return this.Retrieve(r => r.MailMessageSent); }
            set { this.Store(r => r.MailMessageSent, value); }
        }
        public bool SendOnNextPublish {
            get { return this.Retrieve(r => r.SendOnNextPublish); }
            set { this.Store(r => r.SendOnNextPublish, value); }
        }

        public bool SendToTestEmail {
            get { return this.Retrieve(r => r.SendToTestEmail); }
            set { this.Store(r => r.SendToTestEmail, value); }
        }

        public string EmailForTest {
            get { return this.Retrieve(r => r.EmailForTest); }
            set { this.Store(r => r.EmailForTest, value); }
        }

        /// <summary>
        /// Initial number of recipients sent to mailer.
        /// </summary>
        public int RecipientsNumber
        {
            get { return this.Retrieve(r => r.RecipientsNumber); }
            set { this.Store(r => r.RecipientsNumber, value); }
        }

        /// <summary>
        /// Number of sent mails.
        /// </summary>
        public int SentMailsNumber
        {
            get { return this.Retrieve(r => r.SentMailsNumber); }
            set { this.Store(r => r.SentMailsNumber, value); }
        }

        //Properties added on 2017/01/30 to manage manual input of a list of recipientts
        public bool UseRecipientList {
            get { return this.Retrieve(r => r.UseRecipientList); }
            set { this.Store(r => r.UseRecipientList, value); }
        }
        public string RecipientList {
            get { return this.Retrieve(r => r.RecipientList); }
            set { this.Store(r => r.RecipientList, value); }
        }
    }
}