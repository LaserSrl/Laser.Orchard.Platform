using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.ContentSync.Services {

    public interface ISyncService : IDependency {
        void Synchronize(SyncContext context);
    }

}