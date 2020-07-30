using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.ViewModels {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class RequestTypeInfo {
        public RequestTypes Type { get; set; }
        public string UrlTemplate { get; set; }
        public string PayloadTemplate { get; set; }
    }

}