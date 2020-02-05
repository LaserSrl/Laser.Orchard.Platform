using Orchard.Data.Migration;

namespace Laser.Orchard.Translator {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("TranslationRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContainerName")
                .Column<string>("ContainerType", column => column.WithLength(1))
                .Column<string>("Context", column => column.WithLength(4000))
                .Column<string>("Message", column => column.WithLength(4000))
                .Column<string>("TranslatedMessage", column => column.WithLength(4000))
                .Column<string>("Language")
                );
            SchemaBuilder.CreateTable("TranslationFolderSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContainerName")
                .Column<string>("ContainerType", column => column.WithLength(1))
                //.Column<string>("Language")
                .Column<bool>("Deprecated")
                .Column<string>("OutputPath")
                );

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("TranslationFolderSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContainerName")
                .Column<string>("ContainerType", column => column.WithLength(1))
                //.Column<string>("Language")
                .Column<bool>("Deprecated")
                );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("TranslationFolderSettingsRecord", table => table
                .AddColumn<string>("OutputPath")
                );

            return 3;
        }
    }
}