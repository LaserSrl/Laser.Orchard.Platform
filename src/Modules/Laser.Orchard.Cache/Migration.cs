using Laser.Orchard.Cache.Models;
using Orchard.Data.Migration;

namespace Laser.Orchard.Cache {
    public class Migration : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("CacheUrlRecord",
              table => table
               .Column<int>("Id", column => column.PrimaryKey().Identity())
               .Column<int>("CacheDuration", column => column.WithDefault(0))
               .Column<int>("CacheGraceTime", column => column.WithDefault(0))
               .Column<int>("Priority", column => column.WithDefault(0))
               .Column<string>("CacheURL", column => column.WithLength(500))
               .Column<string>("CacheToken", column => column.WithLength(500))
          );
            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.CreateTable(typeof(CacheUrlSetting).Name,
              table => table
               .Column<int>("Id", column => column.PrimaryKey().Identity())
               .Column<bool>("ActiveLog", column => column.WithDefault(false))
               .Column<bool>("PreventDefaultAuthenticatedCache", column => column.WithDefault(false))
                .Column<bool>("PreventDefaultNotContentItemAuthenticatedCache", column => column.WithDefault(false))
              );
            return 2;
        }
    }
}