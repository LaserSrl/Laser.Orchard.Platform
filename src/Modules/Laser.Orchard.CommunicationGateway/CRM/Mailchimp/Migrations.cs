using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class Migrations:DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager
                .AlterPartDefinition("MailchimpSubscriptionPart", part => 
                    part.Attachable(true)
                .WithDescription("Adds the ability to subscribe to one Mailchimp audience."));
            return 1;
        }
    }
}