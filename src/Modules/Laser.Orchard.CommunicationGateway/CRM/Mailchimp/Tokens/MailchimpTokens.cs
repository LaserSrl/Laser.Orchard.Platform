using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Tokens;
using System;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Tokens {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpTextTokens : ITokenProvider {
        private readonly IMailchimpService _mailChimpService;

        public Localizer T { get; set; }

        public MailchimpTextTokens(IMailchimpService mailChimpService) {
            T = NullLocalizer.Instance;
            _mailChimpService = mailChimpService;
        }

        public void Describe(DescribeContext context) {
            context.For("Text", T("Mailchimp"), T("Mailchimp text tokens"))
                   .Token("ToMailchimpSubscriberHash", T("To Mailchimp hash"), T("Converts a string to a Mailchimp subscriber hash"));
        }

        public void Evaluate(EvaluateContext context) {

            context.For<string>("Text")
                   .Token("ToMailchimpSubscriberHash", text => _mailChimpService.ComputeSubscriberHash(text))
                   .Chain("ToMailchimpSubscriberHash", "Text", text => _mailChimpService.ComputeSubscriberHash(text));
        }

    }

    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSiteTokens : ITokenProvider {
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        public MailchimpSiteTokens(IOrchardServices orchardServices, IMailchimpService mailChimpService) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
        }

        public void Describe(DescribeContext context) {
            context.For("Site")
                   .Token("MailchimpDefaultAudience", T("Mailchimp default audience"), T("Gets the Mailchimp default audience defined as site settings."));
        }

        public void Evaluate(EvaluateContext context) {

            context.For("Site", (Func<ISite>)(() => _orchardServices.WorkContext.CurrentSite))
                   .Token("MailchimpDefaultAudience", site => site.As<MailchimpSiteSettings>().DefaultAudience)
                   .Chain("MailchimpDefaultAudience", "Text", site => site.As<MailchimpSiteSettings>().DefaultAudience);
        }

    }
}