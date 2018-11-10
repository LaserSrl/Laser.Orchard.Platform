using Orchard.Data.Migration;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;
namespace Laser.Orchard.Mobile {

    [OrchardFeature("Laser.Orchard.Sms")]
    public class SmsMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("UserPwdRecoveryPartRecord", table => table
            .ContentPartRecord()
            .Column<string>("InternationalPrefix", col => col.WithLength(6))
            .Column<string>("PhoneNumber", col => col.WithLength(16)));
            
            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("UserPwdRecoveryPart"));

            return 1;
        }
    }
}