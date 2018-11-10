using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.TemplateManagement.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class MailCommunicationSettingsVM {
        public IEnumerable<TemplatePart> TemplatesUnsubscribeList { get; set; }
        public MailCommunicationSettingsPart Settings { get; set; }
    }
}