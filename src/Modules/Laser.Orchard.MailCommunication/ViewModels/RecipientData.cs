using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class RecipientData {
        public string Title { get; set; }
        public string EmailAddress { get; set; }
    }
}