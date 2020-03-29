using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Models {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartSettings {
        public string PutPayload { get; set; }
        public string MemberEmail { get; set; }
        public string AudienceId { get; set; }
        public string[] PolicyTextReferences { get; set; }
        public bool NotifySubscriptionResult { get; set; }
    }
}