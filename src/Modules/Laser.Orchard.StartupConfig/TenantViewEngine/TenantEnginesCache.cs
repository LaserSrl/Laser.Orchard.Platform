using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Mvc.ViewEngines.ThemeAwareness;
using System;
using System.Collections.Concurrent;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.TenantViewEngine {
    [OrchardSuppressDependency("Orchard.Mvc.ViewEngines.ThemeAwareness.ConfiguredEnginesCache")]
    public class TenantEnginesCache : 
        ConfiguredEnginesCache, IConfiguredEnginesCache {

        IViewEngine _bare;
        private readonly ShellSettings _shellSettings;
        readonly ConcurrentDictionary<string, IViewEngine> _shallow = new ConcurrentDictionary<string, IViewEngine>();
        readonly ConcurrentDictionary<string, IViewEngine> _deep = new ConcurrentDictionary<string, IViewEngine>();

        public TenantEnginesCache(
            ShellSettings shellSettings) {

            _shellSettings = shellSettings;

            _shallow = new ConcurrentDictionary<string, IViewEngine>();
        }

        public new IViewEngine BindBareEngines(Func<IViewEngine> factory) {
            return _bare ?? (_bare = factory());
        }

        public new IViewEngine BindShallowEngines(string themeName, Func<IViewEngine> factory) {
            return _shallow.GetOrAdd($"{themeName} {_shellSettings.Name}", key => factory());
        }

        public new IViewEngine BindDeepEngines(string themeName, Func<IViewEngine> factory) {
            return _deep.GetOrAdd($"{themeName} {_shellSettings.Name}", key => factory());
        }


    }
}