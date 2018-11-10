using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Events;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Layouts.Helpers;
using System;
using System.Data;

namespace Laser.Orchard.CommunicationGateway {

    public class CoomunicationMigrations : DataMigrationImpl {

        private readonly IUtilsServices _utilsServices;
        private readonly IContentDefinitionEventHandler _contentDefinitionEventHandlers;

        public CoomunicationMigrations(
            IUtilsServices utilsServices,
            IContentDefinitionEventHandler contentDefinitionEventHandlers) {

            _utilsServices = utilsServices;
            _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
        }

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
              "QueryFilterPart",
              b => b
              .Attachable(false)
              );

            ContentDefinitionManager.AlterPartDefinition(
                "CommunicationCampaignPart",
                 b => b
                    .Attachable(false)
                    .WithField("FromDate", cfg => cfg.OfType("DateTimeField").WithDisplayName("From Date"))
                    .WithField("ToDate", cfg => cfg.OfType("DateTimeField").WithDisplayName("To Date"))
            );
            ContentDefinitionManager.AlterTypeDefinition(
              "CommunicationCampaign",
              type => type
                  .WithPart("TitlePart")
                  //.WithPart("AutoroutePart", p => p
                  //  .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                  //  .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                  //  .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of campaign'}]")
                  //  .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                  //     )
                .WithPart("IdentityPart")
                .WithPart("CommunicationCampaignPart")
                  // .WithPart("LocalizationPart")
                .WithPart("CommonPart")
                .Creatable(false)
                .DisplayedAs("Campaign")
              );
            ContentDefinitionManager.AlterPartDefinition(
   "CommunicationAdvertisingPart",
    b => b
       .Attachable(false)
       .WithField("ContentLinked", cfg => cfg
           .OfType("ContentPickerField")
               .WithSetting("ContentPickerFieldSettings.Hint", "Select a ContentItem.")
               .WithSetting("ContentPickerFieldSettings.Required", "False")
               .WithSetting("ContentPickerFieldSettings.Multiple", "False")
               .WithSetting("ContentPickerFieldSettings.ShowContentTab", "True")
               .WithSetting("ContentPickerFieldSettings.ShowSearchTab", "True")
               .WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", "")
               .WithDisplayName("Content")
               .WithSetting("ContentPartSettings.Attachable", "True")
           )
       .WithField("Gallery", cfg => cfg
           .OfType("MediaLibraryPickerField")
            .WithDisplayName("Gallery")
            .WithSetting("MediaLibraryPickerFieldSettings.Required", "false")
            .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "false")
            .WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", "Image")
            .WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
            .WithSetting("MediaLibraryPickerFieldSettings.Hint", "Insert Image")
           )
        );
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
               .WithPart("TitlePart")
                //      .WithPart("BodyPart")
                .WithPart("AutoroutePart", p => p
                   .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                   .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                   .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of Advertising'}]")
                   .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                      )
              .WithPart("IdentityPart")
              .WithPart("CommunicationAdvertisingPart")
              .WithPart("LocalizationPart")
              .WithPart("CommonPart")
              .WithPart("PublishLaterPart")
              .WithPart("QueryFilterPart")
                //  .WithPart("FacebookPostPart")
                //  .WithPart("TwitterPostPart")
              .Creatable(false)
               .Draftable(true)
              .DisplayedAs("Advertising")

            );
            return 1;
        }

        public int UpdateFrom1() {
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .WithPart("TagsPart")
                );
            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition(
                "CommunicationAdvertisingPart",
                b => b
                .WithField("UrlLinked", cfg => cfg
                    .WithSetting("LinkFieldSettings.LinkTextMode", "Static")
                .OfType("LinkField"))
                );
            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition(
              "CommunicationContactPart",
               b => b
               .Attachable(false)
            );
            SchemaBuilder.CreateTable("CommunicationContactPartRecord",
                table => table
                    .ContentPartRecord()
            );

            ContentDefinitionManager.AlterTypeDefinition(
                   "CommunicationContact", type => type
                       .WithPart("TitlePart")
                       .WithPart("CommonPart")
                       .WithPart("IdentityPart")
                       .WithPart("CommunicationContactPart")
                       .WithPart("ProfilePart")
                       .Creatable(false)
                       .Draftable(false)
                   );
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .AddColumn<int>("UserPartRecord_Id")
             );
            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .AddColumn<bool>("Master", col => col.WithDefault(false))
             );
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.CreateTable("CommunicationEmailRecord",
            table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("EmailContactPartRecord_Id", column => column.WithDefault(0))
                .Column<string>("Language", column => column.WithLength(10))
                .Column<bool>("Validated", col => col.WithDefault(true))
                .Column<DateTime>("DataInserimento", c => c.NotNull())
                .Column<DateTime>("DataModifica", c => c.NotNull())
                .Column<bool>("Produzione", col => col.WithDefault(false))
                .Column<string>("Email", column => column.WithLength(400))
             );
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.CreateTable("CommunicationSmsRecord",
            table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("SmsContactPartRecord_Id", column => column.WithDefault(0))
                .Column<string>("Language", column => column.WithLength(10))
                .Column<bool>("Validated", col => col.WithDefault(true))
                .Column<DateTime>("DataInserimento", c => c.NotNull())
                .Column<DateTime>("DataModifica", c => c.NotNull())
                .Column<bool>("Produzione", col => col.WithDefault(false))
                .Column<string>("Sms", column => column.WithLength(400))
                 .Column<string>("Prefix", column => column.WithLength(400))
             );
            return 9;
        }

        public int UpdateFrom9() {
            ContentDefinitionManager.AlterPartDefinition("CommunicationAdvertisingPart",
                alt => alt.RemoveField("Campaign"));
            SchemaBuilder.CreateTable("CommunicationAdvertisingPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("CampaignId"));
            SchemaBuilder.AlterTable("CommunicationAdvertisingPartRecord",
                table => table.CreateIndex("IX_CampaignId", "CampaignId"));
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .CreateIndex("IX_UserPartRecord_Id", "UserPartRecord_Id")
             );
            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.CreateTable("EmailContactPartRecord",
               table => table
                   .ContentPartRecord()
           );
            return 11;
        }

        public int UpdateFrom11() {
            SchemaBuilder.CreateTable("SmsContactPartRecord",
               table => table
                   .ContentPartRecord()
           );
            return 12;
        }

        public int UpdateFrom12() {
            SchemaBuilder.AlterTable("CommunicationEmailRecord",
                table => table
                    .CreateIndex("IX_CommunicationEmailRecord_EmailContactPartRecord_Id", "EmailContactPartRecord_Id"));
            SchemaBuilder.AlterTable("CommunicationSmsRecord",
                table => table
                    .CreateIndex("IX_CommunicationEmailRecord_SmsContactPartRecord_Id", "SmsContactPartRecord_Id"));
            return 13;
        }

        public int UpdateFrom13() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .RemovePart("TagsPart")
                );
            return 14;
        }

        public int UpdateFrom14() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .RemovePart("QueryFilterPart")
                );
            return 15;
        }

        public int UpdateFrom15() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .WithPart("TagsPart")
                );
            return 16;
        }

        public int UpdateFrom16() {
            SchemaBuilder.AlterTable("CommunicationSmsRecord", table => table.AddColumn<bool>("AccettatoUsoCommerciale", c => c.WithDefault(false)));
            SchemaBuilder.AlterTable("CommunicationSmsRecord", table => table.AddColumn<bool>("AutorizzatoTerzeParti", c => c.WithDefault(false)));
            SchemaBuilder.AlterTable("CommunicationEmailRecord", table => table.AddColumn<bool>("AccettatoUsoCommerciale", c => c.WithDefault(false)));
            SchemaBuilder.AlterTable("CommunicationEmailRecord", table => table.AddColumn<bool>("AutorizzatoTerzeParti", c => c.WithDefault(false)));

            return 17;
        }

        public int UpdateFrom17() {
            ContentDefinitionManager.AlterPartDefinition("ExportTaskParametersPart", part => part
                .WithField("Parameters", cfg => cfg.OfType("TextField"))
                );
            ContentDefinitionManager.AlterTypeDefinition("ExportTaskParameters", cfg => cfg
                .WithPart("ExportTaskParametersPart")
                .Creatable(false)
                .Draftable(false));

            return 18;
        }

        public int UpdateFrom18() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .AddColumn<string>("Logs")
             );
            return 19;
        }

        public int UpdateFrom19() {
            _utilsServices.EnableFeature("Laser.Orchard.ZoneAlternates");
            return 20;
        }

        public int UpdateFrom20() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
                   .AlterColumn("Logs", x => x.WithType(DbType.String).Unlimited()));
            return 21;
        }

        public int UpdateFrom21() {
            SchemaBuilder.AlterTable("CommunicationEmailRecord",
                table => table.AddColumn<string>("KeyUnsubscribe", column => column.WithLength(500))
               );
            SchemaBuilder.AlterTable("CommunicationEmailRecord",
                table => table.AddColumn<DateTime>("DataUnsubscribe", column => column.Nullable())
               );

            return 22;
        }

        public int UpdateFrom22() {
            SchemaBuilder.CreateTable("CommunicationDeliveryReportRecord",
            table => table
                .Column<int>("CommunicationAdvertisingPartRecord_Id", column => column.WithDefault(0))
                .Column<string>("ExternalId", column => column.WithLength(50))
                .Column<DateTime>("RequestDate", column => column.Nullable())
                .Column<DateTime>("SubmittedDate", column => column.Nullable())
                .Column<string>("Status", col => col.WithLength(50))
                .Column<string>("Recipient", column => column.Unlimited())
                .Column<string>("Context", column => column.WithLength(100))
                .Column<string>("Medium", column => column.WithLength(50))
             );

            return 23;
        }

        public int UpdateFrom23() {
            ContentDefinitionManager.AlterTypeDefinition(
                  "CommunicationContact", type => type
                   .WithPart("AutoroutePart", part => part
                   .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                   .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                   .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]")
                   .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                   );
            return 24;
        }

        public int UpdateFrom24() {
            ContentDefinitionManager.AlterTypeDefinition(
                  "CommunicationContact", type => type
                   .WithPart("AutoroutePart", part => part
                   .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                   .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                   .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: 'contact_{Content.Slug}', Description: 'Autoroute Contact'}]")
                   .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                   );
            return 25;
        }
        public int UpdateFrom25() {
            SchemaBuilder.CreateTable("CommunicationRetryRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("ContentItemRecord_Id", column => column.WithDefault(0))
                    .Column<string>("Context", column => column.WithLength(100))
                    .Column<int>("NoOfFailures", column => column.WithDefault(0))
                    .Column<string>("Data", column => column.Unlimited())
             );
            return 26;
        }
        public int UpdateFrom26() {
            SchemaBuilder.AlterTable("CommunicationRetryRecord",
                table => table.AddColumn<bool>("PendingErrors", column => column.WithDefault(false))
            );
            return 27;
        }
        public int UpdateFrom27() {
            ContentDefinitionManager.AlterPartDefinition(typeof(ContactInfoPart).Name, p => p.Attachable(true));
            return 28;
        }
        public int UpdateFrom28() {
            ContentDefinitionManager.AlterPartDefinition(typeof(ContactInfoPart).Name, p => p.Attachable(true).Placeable(true));
            return 29;
        }
        public int UpdateFrom29() {
            ContentDefinitionManager.AlterPartDefinition(typeof(ContactInfoPart).Name, p => 
                p.Attachable(true)
                .Placeable(true)
                .WithDescription("Fields that are not synchronized with related user."));
            return 30;
        }

        public int UpdateFrom30() {
            SchemaBuilder.AlterTable("CommunicationDeliveryReportRecord",
               table => table
                   .AddColumn<int>("Id", column => column.PrimaryKey().Identity()));

            return 31;
        }

        /// <summary>
        /// This migration added when we implemented the front end settings for display/
        /// edit controlled by ProfilePart, that need things you want to show on front end to 
        /// be in the actual definitions of ContentTypes.
        /// </summary>
        public int UpdateFrom31() {
            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", builder => builder
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("CommunicationContact", content => content
                .WithPart("FavoriteCulturePart"));
            _contentDefinitionEventHandlers.ContentPartAttached(
                new ContentPartAttachedContext { ContentTypeName = "CommunicationContact", ContentPartName = "FavoriteCulturePart" });

            return 32;
        }
    }
}