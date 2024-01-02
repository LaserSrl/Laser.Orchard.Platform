
using Orchard.Data.Migration;

namespace Laser.Orchard.Cache {
    public class Migration : DataMigrationImpl {
        public int Create() {
            return 3;
        }

        public int UpdateFrom2() {
            // We are dismissing the "old" Laser.Orchard.Cache feature to replace it with
            // other more pointed components.
            SchemaBuilder.DropTable("CacheUrlRecord");
            SchemaBuilder.DropTable("CacheUrlSetting");
            return 3;
        }
    }
}