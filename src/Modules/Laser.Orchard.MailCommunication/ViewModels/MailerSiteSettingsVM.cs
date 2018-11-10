using Laser.Orchard.MailCommunication.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {

    [OrchardFeature("Laser.Orchard.MailerUtility")]
    public class MailerSiteSettingsVM {
        public MailerSiteSettingsPart Settings { get; set; }
    }
}