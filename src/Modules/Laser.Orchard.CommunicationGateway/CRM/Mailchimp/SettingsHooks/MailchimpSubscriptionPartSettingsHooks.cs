using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.SettingsHooks
{

    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class MailchimpSubscriptionPartSettingsHooks : ContentDefinitionEditorEventsBase
    {
        const string TEMPLATE_NAME = "Parts/Mailchimp/MailchimpSubscriptionPartSettings";
        const string PREFIX = "MailchimpSubscriptionPartSettings";

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition)
        {

            if (definition.PartDefinition.Name != "MailchimpSubscriptionPart") yield break;
            var model = definition.Settings.GetModel<MailchimpSubscriptionPartSettings>();
            yield return DefinitionTemplate(model, TEMPLATE_NAME, PREFIX);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel)
        {

            if (builder.Name != "MailchimpSubscriptionPart") yield break;

            var model = new MailchimpSubscriptionPartSettings();
            updateModel.TryUpdateModel(model, PREFIX, null, null);
            builder.WithSetting("MailchimpSubscriptionPartSettings.AudienceId",
                model.AudienceId);
            builder.WithSetting("MailchimpSubscriptionPartSettings.PutPayload",
                model.PutPayload);
            builder.WithSetting("MailchimpSubscriptionPartSettings.MemberEmail",
                model.MemberEmail);
            builder.WithSetting("MailchimpSubscriptionPartSettings.PolicyTextReferences",
                model.PolicyTextReferences != null && model.PolicyTextReferences.Count() > 0 ? string.Join(",", model.PolicyTextReferences) : null);
            builder.WithSetting("MailchimpSubscriptionPartSettings.NotifySubscriptionResult",
                model.NotifySubscriptionResult.ToString());


            yield return DefinitionTemplate(model, TEMPLATE_NAME, PREFIX);
        }
    }
}