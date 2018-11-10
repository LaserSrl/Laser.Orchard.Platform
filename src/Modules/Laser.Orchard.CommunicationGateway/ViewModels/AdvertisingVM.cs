using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.ViewModels {

    public class partTwitterPost {
        public string Message { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int[] AccountList { get; set; }
    }

    public class partFacebookPost {
        public string Message { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int[] AccountList { get; set; }
    }

    public class partMailCommunication {
        public int IdTemplate { get; set; }
    }

    public class partMobilePush {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Device { get; set; }
    }

    public class partSmsGateway {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }

    public class Advertising {
        public string Title { get; set; }
        public partTwitterPost TwitterPost { get; set; }
        public partFacebookPost FacebookPost { get; set; }
        public partMailCommunication MailCommunication { get; set; }
        public partMobilePush MobilePush { get; set; }
        public partSmsGateway SmsGateway { get; set; }
        public DateTime DatePublish { get; set; }
    }

    public class AdvertisingCommunication {
        public Advertising Advertising { get; set; }
    }

    public class AdvertisingCommunicationAPIResult {
        public int Id { get; set; }
        public string Error { get; set; }
        public string Information { get; set; }
    }
}