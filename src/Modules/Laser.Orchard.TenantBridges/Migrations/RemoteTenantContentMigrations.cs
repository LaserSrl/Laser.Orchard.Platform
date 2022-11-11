using Laser.Orchard.TenantBridges.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TenantBridges.Migrations {
    public class RemoteTenantContentMigrations : DataMigrationImpl {
        public int Create() {

            var contentSnippetPartName = typeof(RemoteTenantContentSnippetWidgetPart).Name;
            ContentDefinitionManager
                .AlterPartDefinition(contentSnippetPartName, 
                    part => part
                        .Attachable()
                        .WithDescription("Use this part to configure the content we want to fetch from a remote tenant."));

            ContentDefinitionManager
                .AlterTypeDefinition("RemoteContentSnippetWidget",
                    cfg => cfg
                        .AsWidgetWithIdentity()
                        .WithPart(contentSnippetPartName));
                
            return 1;
        }
    }
}