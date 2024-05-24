using Newtonsoft.Json.Linq;
using Orchard;

namespace Laser.Orchard.StartupConfig.Providers {
    public interface IJsonDataTableColumnProvider : IDependency {
         JToken ProcessColumnDefinition(JToken columnDefinition);
    }
}
