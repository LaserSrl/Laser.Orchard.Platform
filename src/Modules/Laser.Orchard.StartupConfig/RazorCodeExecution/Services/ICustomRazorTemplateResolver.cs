using Laser.Orchard.StartupConfig.RazorBase.Models;
using Orchard;
using RazorEngine.Templating;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {
    public interface ICustomRazorTemplateResolver : IDependency {
        /// <summary>
        /// This is the priority the provider will be ran with.
        /// A higher number means that the provider will be executed first.
        /// </summary>
        int Priority { get; }

        void GetKeyName(KeyNameResolveContext context);

        void ResolveTemplateSource(TemplateSourceResolveContext context);
    }
}
