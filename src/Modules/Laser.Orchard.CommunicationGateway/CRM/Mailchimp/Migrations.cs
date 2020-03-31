using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp {
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