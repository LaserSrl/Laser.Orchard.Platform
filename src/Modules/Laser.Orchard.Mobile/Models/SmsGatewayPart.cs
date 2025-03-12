using Laser.Orchard.Mobile.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Laser.Orchard.Mobile.Models {
    public class SmsGatewayPartRecord : ContentPartRecord {

        public virtual string Message { get; set; }
        public virtual bool HaveAlias { get; set; }
        public virtual string Alias { get; set; }
        public virtual bool SmsMessageSent { get; set; }
        public virtual bool SendToTestNumber { get; set; }
        public virtual string NumberForTest { get; set; }
        public virtual bool SendOnNextPublish { get; set; }
        public virtual int SmsDeliveredOrAcceptedNumber { get; set; }
        public virtual int SmsRejectedOrExpiredNumber { get; set; }
        public virtual int SmsRecipientsNumber { get; set; }
        public virtual string PrefixForTest { get; set; }
        public virtual string RecipientList { get; set; }
        public virtual string ExternalId { get; set; }
        public virtual bool SendToRecipientList { get; set; }
    }

    public class SmsGatewayPart : ContentPart<SmsGatewayPartRecord> {

        public string Message {
            get { return this.Retrieve(x => x.Message); }
            set { this.Store(x => x.Message, value); }
        }

        public bool HaveAlias {
            get { return this.Retrieve(x => x.HaveAlias); }
            set { this.Store(x => x.HaveAlias, value); }
        }

        public string Alias {
            get { return this.Retrieve(x => x.Alias); }
            set { this.Store(x => x.Alias, value); }
        }

        public bool SmsMessageSent {
            get { return this.Retrieve(x => x.SmsMessageSent); }
            set { this.Store(x => x.SmsMessageSent, value); }
        }

        public bool SendToTestNumber {
            get { return this.Retrieve(x => x.SendToTestNumber); }
            set { this.Store(x => x.SendToTestNumber, value); }
        }

        [SmsValidationService]
        public string NumberForTest {
            get { return this.Retrieve(x => x.NumberForTest); }
            set { this.Store(x => x.NumberForTest, value); }
        }

        public bool SendOnNextPublish {
            get { return this.Retrieve(x => x.SendOnNextPublish); }
            set { this.Store(x => x.SendOnNextPublish, value); }
        }

        public int SmsDeliveredOrAcceptedNumber {
            get { return this.Retrieve(x => x.SmsDeliveredOrAcceptedNumber); }
            set { this.Store(x => x.SmsDeliveredOrAcceptedNumber, value); }
        }

        public int SmsRejectedOrExpiredNumber {
            get { return this.Retrieve(x => x.SmsRejectedOrExpiredNumber); }
            set { this.Store(x => x.SmsRejectedOrExpiredNumber, value); }
        }

        public int SmsRecipientsNumber {
            get { return this.Retrieve(x => x.SmsRecipientsNumber); }
            set { this.Store(x => x.SmsRecipientsNumber, value); }
        }

        [SmsValidationService]
        public string PrefixForTest {
            get { return this.Retrieve(x => x.PrefixForTest); }
            set { this.Store(x => x.PrefixForTest, value); }
        }

        [StringLengthMax]
        public string RecipientList {
            get { return this.Retrieve(x => x.RecipientList); }
            set { this.Store(x => x.RecipientList, value); }
        }

        public string ExternalId {
            get { return this.Retrieve(x => x.ExternalId); }
            set { this.Store(x => x.ExternalId, value); }
        }

        public bool SendToRecipientList {
            get { return this.Retrieve(x => x.SendToRecipientList); }
            set { this.Store(x => x.SendToRecipientList, value); }
        }

    }
}