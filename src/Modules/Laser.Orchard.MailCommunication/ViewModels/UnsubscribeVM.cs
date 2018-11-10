using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class UnsubscribeVM {

        public string Email { get; set; }
        public string LinkUnsubscription { get; set; }
        public string KeyUnsubscription { get; set; }
        public DateTime UnsubscriptionDate { get; set; }

    }
}