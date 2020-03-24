using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.NwazetIntegration.Migration {
    public class MigrationsGTM : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("GTMProductPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("ProductId")
                .Column<string>("Name")
                .Column<string>("Brand")
                .Column<string>("Category")
                .Column<string>("Variant")
                .Column<decimal>("Price")
                );
            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
                "GTMProductPart",
                b => b
                    .Attachable(true)
                    .WithDescription("Google Tag Manager")
            );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("AddedMeasuringPurchase",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("OrderPartRecord_Id")
                    .Column<bool>("AddedScript")
                );
            return 3;
        }
    }
}