using Laser.Orchard.Commons.Core;
using Laser.Orchard.Commons.Enums;
using Laser.Orchard.Highlights.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Highlights {

    public class Migrations : DataMigrationImpl {

        public int Create() {

            #region [Schema]

            SchemaBuilder
              .CreateTable("HighlightsItemPartRecord",
                           table => table
                                      .ContentPartRecord()
                                      .Column<string>("Sottotitolo", command => command.WithLength(255))
                                      .Column<string>("TitleSize", col => col.WithLength(25))
                                      .Column<int>("HighlightsGroupPartRecordId", col => col.NotNull())
                                      .Column<bool>("Video", col => col.WithDefault(false))
                                      .Column<string>("LinkUrl", col => col.WithLength(500))
                                      .Column<string>("LinkTarget", col => col.WithLength(25))
                                      .Column<string>("LinkText", c => c.WithLength(1000))
                                      .Column<int>("ItemOrder"));


            SchemaBuilder
              .CreateTable("HighlightsGroupPartRecord",
                           table => table
                                      .ContentPartRecord()
                                      .Column<string>("DisplayPlugin", c => c.WithLength(25))
                                      .Column<string>("DisplayTemplate", col => col.WithLength(25))
                                      );

            #endregion

            #region [ Parts ]

            ContentDefinitionManager
            .AlterPartDefinition("HighlightsItemPart",
                               part => part
                                          .TextField("MediaText", TextFieldFlavor.Html, false, "Testo", "Testo da visualizzare.")
                                          .MediaLibraryPickerField("MediaObject", false, false, null, "Oggetto", "Oggetto da includere.")
                                          .WithDescription("Evidenza con oggetto, testo e collegamento web.")
                               );

            ContentDefinitionManager
              .AlterPartDefinition("HighlightsGroupPart",
                                   part => part
                                             .Attachable());

            #endregion

            #region [ Types ]

            ContentDefinitionManager
              .AlterTypeDefinition("HighlightsItem",
                                   item => item
                                             .CommomPart(true, true)
                                             .WithPart("TitlePart")
                                             .WithPart("HighlightsItemPart")
                                             .WithPart("PublishLaterPart")
                                             .WithPart("ArchiveLaterPart")
                                             .WithPart("IdentityPart")
                                       //.WithPart("LocalizationPart")
                                             .Draftable());

            ContentDefinitionManager
              .AlterTypeDefinition("HighlightsGroupWidget",
                                   item => item
                                             .CommomPart(true, true)
                                             .WithPart("HighlightsGroupPart")
                                             .WithPart("WidgetPart")
                                             .WithPart("IdentityPart")
                                             .WithSetting("Stereotype", "Widget"));

            #endregion

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("HighlightsGroupPartRecord", table => table
                .AddColumn<string>("ItemsSourceType", c => c.WithLength(30).WithDefault("ByHand"))
                );
            SchemaBuilder.AlterTable("HighlightsGroupPartRecord", table => table
                .AddColumn<int>("Query_Id")
                );
            return 2;
        }

    }
}