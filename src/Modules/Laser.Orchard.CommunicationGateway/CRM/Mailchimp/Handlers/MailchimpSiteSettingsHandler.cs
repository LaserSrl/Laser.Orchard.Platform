﻿using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Handlers {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteSettingsHandler : ContentHandler {
        public MailchimpSiteSettingsHandler(IMailchimpService service) {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<MailchimpSiteSettings>("Site"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Mailchimp")));
        }
    }
}