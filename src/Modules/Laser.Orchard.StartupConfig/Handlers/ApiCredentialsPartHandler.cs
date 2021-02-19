using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsPartHandler : ContentHandler {
        public ApiCredentialsPartHandler(IRepository<ApiCredentialsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<ApiCredentialsPart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}