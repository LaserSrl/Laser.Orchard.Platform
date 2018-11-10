using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;

namespace Laser.Orchard.StartupConfig.Services {
    public interface ICurrentContentAccessor : IDependency {
        ContentItem CurrentContentItem { get; }
        int? CurrentContentItemId { get; }
    }
}