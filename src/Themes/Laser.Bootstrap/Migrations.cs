using Orchard.Data.Migration;

namespace Laser.Bootstrap {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ThemeSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Swatch", c => c.WithLength(50).WithDefault(Constants.SwatchCssDefault))
                .Column<bool>("UseFixedNav")
                .Column<bool>("UseNavSearch")
                .Column<bool>("UseFluidLayout")
                .Column<bool>("UseInverseNav")
                .Column<bool>("UseStickyFooter")
            );

            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ThemeSettingsRecord", table=>table
                .AddColumn<string>("TagLineText", column=>column.WithLength(75).WithDefault("Your Tag Line Here")));
            return 2;
        }
    }
}