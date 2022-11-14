using Laser.Orchard.TenantBridges.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.TenantBridges.Services {
    public interface IRemoteContentService : IDependency {

        string GetSnippet(RemoteTenantContentSnippetWidgetPart part);
    }
}
