using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Models {
    
    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsSettingsPart : ContentPart{

        ///// <summary>
        ///// URL SMS ENGINE
        ///// </summary>
        //public string UrlSmsEngine {
        //    get { return this.Retrieve(x => x.UrlSmsEngine); }
        //    set { this.Store(x => x.UrlSmsEngine, value); }
        //}
        /// <summary>
        /// MAM DRIVER ID
        /// </summary>
        [Required]
        public string MamDriverIdentifier {
            get { return this.Retrieve(x => x.MamDriverIdentifier); }
            set { this.Store(x => x.MamDriverIdentifier, value); }
        }
        /// <summary>
        /// Numero Totale Retry Massimo
        /// </summary>
        public Nullable<int> TotalMaxRetry {
            get { return this.Retrieve(x => x.TotalMaxRetry); }
            set { this.Store(x => x.TotalMaxRetry, value); }
        }
        /// <summary>
        /// Numero Totale Retry Massimo in secondi
        /// </summary>
        public Nullable<int> RetryInterval {
            get { return this.Retrieve(x => x.RetryInterval); }
            set { this.Store(x => x.RetryInterval, value); }
        }
        /// <summary>
        /// Sm sFrom
        /// </summary>
        public string SmsFrom {
            get { return this.Retrieve(x => x.SmsFrom); }
            set { this.Store(x => x.SmsFrom, value); }
        }
        /// <summary>
        /// Sms Validity Period
        /// </summary>
        public Nullable<int> SmsValidityPeriod {
            get { return this.Retrieve(x => x.SmsValidityPeriod); }
            set { this.Store(x => x.SmsValidityPeriod, value); }
        }
        /// <summary>
        /// Mam Have Alias
        /// </summary>
        public bool MamHaveAlias {
            get { return this.Retrieve(x => x.MamHaveAlias); }
            set { this.Store(x => x.MamHaveAlias, value); }
        }

        public int MaxLenghtSms {
            get { return this.Retrieve(x => x.MaxLenghtSms); }
            set { this.Store(x => x.MaxLenghtSms, value); }
        }

        public SmsServiceReference.enumProtocollo Protocollo {
            get { return this.Retrieve(x => x.Protocollo); }
            set { this.Store(x => x.Protocollo, value); }
        }

        /// <summary>
        /// Sms Prority
        /// </summary>
        public Nullable<int> SmsPrority {
            get { return this.Retrieve(x => x.SmsPrority); }
            set { this.Store(x => x.SmsPrority, value); }
        }

        /// <summary>
        /// Sms Service End Point
        /// </summary>
        [Required]
        public string SmsServiceEndPoint{
            get { return this.Retrieve(x => x.SmsServiceEndPoint); }
            set { this.Store(x => x.SmsServiceEndPoint, value); }
        }


        public string WsUsername {
            get { return this.Retrieve(x => x.WsUsername); }
            set { this.Store(x => x.WsUsername, value); }
        }

        public string WsPassword {
            get { return this.Retrieve(x => x.WsPassword); }
            set { this.Store(x => x.WsPassword, value); }
        }

    }
}
