using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.ViewModels {

    public class SmsDeliveryReportResultVM {
        public string ReportStatus { get; set; }
        public List<SmsDeliveryReportDetails> Details { get; set; }
    }

    public class SmsDeliveryReportDetails {
        public string Recipient { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string Status { get; set; }
    }

}