using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.SEO {

  [OrchardFeature("Laser.Orchard.Favicon")]
  public class FaviconMigrations : DataMigrationImpl {

    public int Create() {
      SchemaBuilder.CreateTable(
          "FaviconSettingsRecord",
          table => table
                       .ContentPartRecord()
                       .Column<string>("FaviconUrl")
          );
      return 1;
    }

  }
}