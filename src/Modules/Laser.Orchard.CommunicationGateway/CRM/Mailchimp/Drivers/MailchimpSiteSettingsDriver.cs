using Laser.Orchard.CommunicationGateway.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.Mailchimp.Services;
using Laser.Orchard.CommunicationGateway.Mailchimp.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Drivers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteSettingsDriver : ContentPartDriver<MailchimpSiteSettings> {
        private const string TemplateName = "Parts/Mailchimp/MailchimpSiteSettings";
        private readonly IMailchimpService _service;

        public MailchimpSiteSettingsDriver(IMailchimpService service) {
            T = NullLocalizer.Instance;
            _service = service;
        }

        public Localizer T { get; set; }


        protected override string Prefix { get { return "MailchimpSiteSettings"; } }

        protected override DriverResult Editor(MailchimpSiteSettings part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MailchimpSiteSettings part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MailchimpSiteSettings_Edit", () => {
                var model = new MailchimpSiteSettingsEdit {
                    ApiKey = part.ApiKey
                };
                if (updater != null) {
                    updater.TryUpdateModel(model, Prefix, null, null);
                    if (!string.IsNullOrWhiteSpace(model.NewApiKey)) {
                        part.ApiKey = _service.CryptApiKey(model.NewApiKey);
                    }
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
            }).OnGroup("Mailchimp");
        }
    }
}