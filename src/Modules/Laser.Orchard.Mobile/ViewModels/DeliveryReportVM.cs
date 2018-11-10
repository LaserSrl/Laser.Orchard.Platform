using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Laser.Orchard.Mobile.ViewModels {

    [System.SerializableAttribute()]
    [XmlRootAttribute(IsNullable = false, ElementName = "XML")]
    public class DeliveryReportVM {
        /* 
           <?xml version="1.0" encoding="UTF-8" ?>
           <XML><MESSAGE_IDENTIFIER>MONITBLPA_9681</MESSAGE_IDENTIFIER><DRIVERID>1018_TESTCIT@MAMPA</DRIVERID><MESSAGEID>1002000013703</MESSAGEID><SMSCOMPONENTID>6fc09862-56f4-406f-a8e3-66f50d596f26</SMSCOMPONENTID><REQUEST_DATE>11/03/2012 1.43.42</REQUEST_DATE><STATO>DELIVERED</STATO><SUBMITTED_DATE>11/03/2012 1.43.42</SUBMITTED_DATE><TO>393404399991</TO></XML>
        */

        private string messageIdentifier;

        [XmlElementAttribute(ElementName = "MESSAGE_IDENTIFIER")]
        public string MessageIdentifier {
            get { return messageIdentifier; }
            set { messageIdentifier = value; }
        }
        private string body;

        [XmlElementAttribute(ElementName = "BODY")]
        public string Body {
            get { return body; }
            set { body = value; }
        }

        private string driverId;

        [XmlElementAttribute(ElementName = "DRIVERID")]
        public string DriverId {
            get { return driverId; }
            set { driverId = value; }
        }
        private string messageId;

        [XmlElementAttribute(ElementName = "MESSAGEID")]
        public string MessageId {
            get { return messageId; }
            set { messageId = value; }
        }


        private string smsComponentId;

        [XmlElementAttribute(ElementName = "SMSCOMPONENTID")]
        public string SmsComponentId {
            get { return smsComponentId; }
            set { smsComponentId = value; }
        }

        private string requestDate;

        [XmlElementAttribute(ElementName = "REQUEST_DATE")]
        public string RequestDate {
            get { return requestDate; }
            set { requestDate = value; }
        }

        private string stato;

        [XmlElementAttribute(ElementName = "STATO")]
        public string Stato {
            get { return stato; }
            set { stato = value; }
        }

        private string subMittedDate;

        [XmlElementAttribute(ElementName = "SUBMITTED_DATE")]
        public string SubMittedDate {
            get { return subMittedDate; }
            set { subMittedDate = value; }
        }

        private string to;

        [XmlElementAttribute(ElementName = "TO")]
        public string To {
            get { return to; }
            set { to = value; }
        }


        private string testoSms;

        [XmlElementAttribute(ElementName = "TESTO_SMS")]
        public string TestoSms {
            get { return testoSms; }
            set { testoSms = value; }
        }

    }
}