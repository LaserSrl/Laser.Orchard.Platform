using Orchard.Data.Migration;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SMSGatewayMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("SmsGatewayPartRecord", table => table
            .ContentPartRecord()
            .Column<string>("Message", col => col.Unlimited())
            .Column<bool>("HaveAlias", col => col.WithDefault(false))
            .Column<string>("Alias", col => col.WithLength(100))
            .Column<bool>("SmsMessageSent", col => col.WithDefault(false)));

            ContentDefinitionManager.AlterPartDefinition("SmsGatewayPart", config => config.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("CommunicationAdvertising", config => config
                .WithPart("SmsGatewayPart"));

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SendToTestNumber", System.Data.DbType.Boolean, col => col.WithDefault(false)));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("NumberForTest", System.Data.DbType.String, col => col.WithLength(50)));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SendOnNextPublish", System.Data.DbType.Boolean, col => col.WithDefault(false)));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SmsDeliveredOrAcceptedNumber", System.Data.DbType.Int32, col => col.WithDefault(0)));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SmsRejectedOrExpiredNumber", System.Data.DbType.Int32, col => col.WithLength(0)));
            
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SmsRecipientsNumber", System.Data.DbType.Int32, col => col.WithLength(0)));

            
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("PrefixForTest", System.Data.DbType.String, col => col.WithLength(5)));

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("RecipientList", System.Data.DbType.String, col => col.Unlimited()));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("ExternalId", System.Data.DbType.String, col => col.WithLength(50)));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SendToRecipientList", System.Data.DbType.Boolean, col => col.WithDefault(false)));

            return 6;
        }
    }
}