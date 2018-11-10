using Laser.Orchard.Events.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;
using System.Data;

namespace Laser.Orchard.Events
{
    public class Migrations : DataMigrationImpl
    {
        private readonly IUtilsServices _utilServices;

        public Migrations(IUtilsServices utilsServices)
        {
            _utilServices = utilsServices;
        }

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create()
        {
            // Creating table ActivityPartRecord
            SchemaBuilder.CreateTable("ActivityPartRecord", table => table
                .ContentPartRecord()
                .Column<DateTime>("DateTimeStart")
                .Column<DateTime>("DateTimeEnd")
                .Column<bool>("AllDay")
                .Column<bool>("Repeat")
                .Column<string>("RepeatType", column => column.WithLength(1))
                .Column<int>("RepeatValue")
                .Column<string>("RepeatDetails")
                .Column<bool>("RepeatEnd")
                .Column<DateTime>("RepeatEndDate")
            );

            //Creating the ActivityPart
            ContentDefinitionManager.AlterPartDefinition(
                typeof(ActivityPart).Name,
                part => part
                            .Attachable()
                            .WithDescription("Used by the Event content type. Contains the data telling when an event must be displayed.")
            );

            //Creating the fields for the content type Event
            ContentDefinitionManager.AlterPartDefinition(
                "CalendarEvent",
                part => part
                            .WithField("Gallery",
                                        field => field
                                                       .OfType("MediaLibraryPickerField")
                                                       .WithDisplayName("Gallery"))
                            .WithField("Relatedevents",
                                        field => field
                                                       .OfType("ContentPickerField")
                                                       .WithDisplayName("Related events"))
            );

            //Creating the content type Event
            ContentDefinitionManager.AlterTypeDefinition(
                "CalendarEvent",
                type => type
                           .WithPart("TitlePart")
                           .WithPart("AutoroutePart")
                           .WithPart("CommonPart")
                           .WithPart("BodyPart")
                           .WithPart("CalendarEvent")
                           .WithPart("LocalizationPart")
                           .WithPart("ActivityPart")
                           .Creatable()
            );

            //Creating the table CalendarPartRecord
            SchemaBuilder.CreateTable("CalendarPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("QueryPartRecord_Id")
                .Column<int>("LayoutRecord_Id")
                .Column<int>("ItemsPerPage")
                .Column<string>("PagerSuffix", column => column.WithLength(255))
                .Column<bool>("DisplayPager")
            );

            //Creating the CalendarPart
            ContentDefinitionManager.AlterPartDefinition(
                typeof(CalendarPart).Name,
                part => part
                            .Attachable()
                            .WithDescription("Used by the Calendar content type. Contains the source query that retrieves the events and some information about how to display them.")
            );

            //Creating the content type Calendar
            ContentDefinitionManager.AlterTypeDefinition(
                "Calendar",
                type => type
                            .WithPart("TitlePart")
                            .WithPart("AutoroutePart")
                            .WithPart("CommonPart")
                            .WithPart("BodyPart")
                            .WithPart("LocalizationPart")
                            .WithPart("CalendarPart")
                            .Creatable()
            );

            //Creating the Calendar widget
            ContentDefinitionManager.AlterTypeDefinition("CalendarWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CalendarPart")
                    .WithSetting("Stereotype", "Widget")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable("CalendarPartRecord", table => table.AddColumn("CalendarShape", DbType.String, column => column.WithLength(5)));
            SchemaBuilder.AlterTable("CalendarPartRecord", table => table.AddColumn("StartDate", DbType.String));
            SchemaBuilder.AlterTable("CalendarPartRecord", table => table.AddColumn("NumDays", DbType.String));

            return 2;
        }
        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("Calendar", type => type.Listable());
            ContentDefinitionManager.AlterTypeDefinition("CalendarEvent", type => type.Listable());
            return 3;
        }
    }
}