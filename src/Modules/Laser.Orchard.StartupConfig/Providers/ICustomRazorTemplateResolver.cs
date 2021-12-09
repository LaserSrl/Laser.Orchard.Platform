using Orchard;
using RazorEngine.Templating;

namespace Laser.Orchard.StartupConfig.Providers {
    public interface ICustomRazorTemplateResolver : IDependency {
        string GetKeyName(string name, ResolveType resolveType, ITemplateKey context);

        string GetTemplateSource(ITemplateKey key);
    }
}
