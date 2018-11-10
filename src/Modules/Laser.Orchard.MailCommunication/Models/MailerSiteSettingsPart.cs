using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.Models
{
    [OrchardFeature("Laser.Orchard.MailerUtility")]
    public class MailerSiteSettingsPart : ContentPart
    {
        /// <summary>
        /// Max number of recipients for each JSON file.
        /// </summary>
        public int RecipientsPerJsonFile
        {
            get { return this.Retrieve(x => x.RecipientsPerJsonFile); }
            set { this.Store(x => x.RecipientsPerJsonFile, value); }
        }

        /// <summary>
        /// Mail priority to be set on each mail.
        /// </summary>
        public MailPriorityValues MailPriority
        {
            get { return this.Retrieve(x => x.MailPriority); }
            set { this.Store(x => x.MailPriority, value); }
        }

        public string FtpHost
        {
            get { return this.Retrieve(x => x.FtpHost); }
            set { this.Store(x => x.FtpHost, value); }
        }

        public string FtpUser
        {
            get { return this.Retrieve(x => x.FtpUser); }
            set { this.Store(x => x.FtpUser, value); }
        }

        public string FtpPassword
        {
            get { return this.Retrieve(x => x.FtpPassword); }
            set { this.Store(x => x.FtpPassword, value); }
        }

        /// <summary>
        /// Remote path to use for FTP transfer.
        /// A trailing slash is added automatically if needed.
        /// </summary>
        public string FtpPath
        {
            get { return this.Retrieve(x => x.FtpPath); }
            set {
                string aux = value ?? "";
                if (aux.EndsWith("/") == false)
                {
                    aux += "/";
                }
                this.Store(x => x.FtpPath, aux); 
            }
        }

        /// <summary>
        /// Validity of token (in days) used to return the number of mails sent.
        /// </summary>
        public int TokenValidity
        {
            get { return this.Retrieve(x => x.TokenValidity); }
            set { this.Store(x => x.TokenValidity, value); }
        }

    }
}