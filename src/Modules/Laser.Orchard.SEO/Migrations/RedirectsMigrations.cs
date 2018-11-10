using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.SEO {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectsMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("RedirectRule", table => table
                .Column<int>("Id", c => c
                    .PrimaryKey()
                    .Identity())
                .Column<DateTime>("CreatedDateTime", c => c.NotNull().WithDefault("GetDate()"))
                .Column<string>("SourceUrl", c => c.NotNull().WithDefault("").WithLength(32768)) //max url lenght for chrome (larger than other browsers as of nov 2016)
                .Column<string>("DestinationUrl", c => c.NotNull().WithDefault("").WithLength(32768))
                .Column<bool>("IsPermanent", c => c.NotNull().WithDefault(false))
                );

            return 1;
        }
    }
}