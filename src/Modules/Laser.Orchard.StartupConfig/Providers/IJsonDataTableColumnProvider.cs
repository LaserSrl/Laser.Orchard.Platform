using Laser.Orchard.StartupConfig.Models;
using Newtonsoft.Json.Linq;
using NHibernate.Linq.Functions;
using Orchard;

namespace Laser.Orchard.StartupConfig.Providers {
    public interface IJsonDataTableColumnProvider : IDependency {
        JToken ProcessColumnDefinition(JToken columnDefinition);
        bool CheckColumnEditor(string editor);
    }
}
