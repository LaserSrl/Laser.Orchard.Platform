using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Contrib.Voting {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("VoteRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("CreatedUtc")
                    .Column<int>("ContentItemRecord_id")
                    .Column<string>("ContentType")
                    .Column<string>("Username")
                    .Column<string>("Hostname")
                    .Column<double>("Value")
                    .Column<string>("Dimension", c => c.Nullable())
                );

            SchemaBuilder.CreateTable("ResultRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("CreatedUtc", column => column.Nullable())
                    .Column<int>("ContentItemRecord_id")
                    .Column<string>("ContentType")
                    .Column<string>("Dimension", c => c.Nullable())
                    .Column<double>("Value")
                    .Column<int>("Count")
                    .Column<string>("FunctionName")
                );

            SchemaBuilder.CreateTable("VoteWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("ContentType")
                    .Column<bool>("Ascending")
                    .Column<int>("Count")
                    .Column<string>("Dimension")
                    .Column<string>("FunctionName")
                );

            ContentDefinitionManager.AlterTypeDefinition("VoteWidget",
                cfg => cfg
                    .WithPart("VoteWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 2;
        }

        public int UpdateFrom1() {

            SchemaBuilder.AlterTable("ResultRecord",
                table => table
                    .DropColumn("Axe")
                );

            SchemaBuilder.AlterTable("ResultRecord",
                table => table
                    .AddColumn<string>("Dimension", c => c.Nullable())
                );

            SchemaBuilder.AlterTable("VoteRecord",
                table => table
                    .DropColumn("Axe")
                );

            SchemaBuilder.AlterTable("VoteRecord",
                table => table
                    .AddColumn<string>("Dimension", c => c.Nullable())
                );

            SchemaBuilder.CreateTable("VoteWidgetPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("ContentType")
                    .Column<bool>("Ascending")
                    .Column<int>("Count")
                    .Column<string>("Dimension")
                    .Column<string>("FunctionName")
                );

            ContentDefinitionManager.AlterTypeDefinition("VoteWidget",
                cfg => cfg
                    .WithPart("VoteWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );
      
            return 2;
        }
    }
}
