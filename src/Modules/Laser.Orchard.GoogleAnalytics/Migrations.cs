using Laser.Orchard.GoogleAnalytics.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Data.Migration;

namespace Laser.Orchard.GoogleAnalytics {
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices _services;

        public Migrations(IOrchardServices services) {
            _services = services;
        }

        public int Create() {
			SchemaBuilder.CreateTable("GoogleAnalyticsSettingsPartRecord", 
				table => table
					.ContentPartRecord()
					.Column<string>("GoogleAnalyticsKey")
					.Column<string>("DomainName")
					.Column<bool>("UseAsyncTracking")
					.Column<bool>("TrackOnAdmin")
				);
			return 1;
		}

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("GoogleAnalyticsSettingsPartRecord",
                table => table.AddColumn<bool>("AnonymizeIp"));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("GoogleAnalyticsSettingsPartRecord",
                table => table.AddColumn<bool>("TrackOnFrontEnd", c => c.NotNull().WithDefault(true)));

            var settings = _services.WorkContext.CurrentSite.As<GoogleAnalyticsSettingsPart>();
            settings.As<InfosetPart>().Store("GoogleAnalyticsSettingsPart", "TrackOnFrontEnd", true);

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("GoogleAnalyticsSettingsPartRecord",
                table => table.AddColumn<bool>("UseTagManager", c => c.NotNull().WithDefault(false)));
            return 4;
        }
    }
}