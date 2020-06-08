using Orchard.ContentManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteSettingsEdit {

        public string NewApiKey { get; set; }
        public string ApiKey { get; set; }
    }
}