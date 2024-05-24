using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IJsonDataTableService : IDependency {
        string ProcessColumnsDefinition(string columnsDefinition);
    }
}
