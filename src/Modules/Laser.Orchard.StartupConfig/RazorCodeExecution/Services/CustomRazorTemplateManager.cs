using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.RazorBase.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Environment.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {
    public class CustomRazorTemplateManager : ITemplateManager {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource> _dynamicTemplates =
            new System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource>();
        private ITemplateManager _defaultTemplateManager;

        public CustomRazorTemplateManager(ShellSettings shellSettings,
            IOrchardServices orchardServices) {
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _defaultTemplateManager = new DelegateTemplateManager();
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source) {
            _dynamicTemplates.AddOrUpdate(key, source, (k, oldSource) => {
                if (oldSource.Template != source.Template) {
                    throw new InvalidOperationException("The same key was already used for another template!");
                }
                return source;
            });
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context) {
            var ctx = new KeyNameResolveContext() {
                OriginalName = name,
                ResolveType = resolveType,
                Context = context,
                ResolvedName = name,
                NameResolved = false
            };

            // I need to read resolvers from WorkContext here, because if I injected them in the class constructor they wouldn't work properly.
            // They cache their content throughout multiple requests and fail to update content templates when needed.
            // This happens because IRazorTemplateManager inherits from ISingletonDependency.
            var resolvers = _orchardServices.WorkContext
                    .Resolve<IEnumerable<ICustomRazorTemplateResolver>>()
                    .OrderByDescending(r => r.Priority);
            foreach (var res in resolvers) {
                if (ctx.NameResolved) {
                    break;
                }
                res.GetKeyName(ctx);
            }

            return new NameOnlyTemplateKey(ctx.ResolvedName, resolveType, context);
        }

        public ITemplateSource Resolve(ITemplateKey key) {
            ITemplateSource result;
            if (_dynamicTemplates.TryGetValue(key, out result)) {
                return result;
            }

            var ctx = new TemplateSourceResolveContext() {
                Key = key,
                Source = string.Empty,
                FileName = key.Name
            };

            if (!string.IsNullOrWhiteSpace(key.Name)) {
                // I need to read resolvers from WorkContext here, because if I injected them in the class constructor they wouldn't work properly.
                // They cache their content throughout multiple requests and fail to update content templates when needed.
                // This happens because IRazorTemplateManager inherits from ISingletonDependency.
                var resolvers = _orchardServices.WorkContext
                    .Resolve<IEnumerable<ICustomRazorTemplateResolver>>()
                    .OrderByDescending(r => r.Priority);
                foreach (var res in resolvers) {
                    if (!string.IsNullOrWhiteSpace(ctx.Source)) {
                        break;
                    }
                    res.ResolveTemplateSource(ctx);
                }
            }

            if (string.IsNullOrWhiteSpace(ctx.Source)) {
                return _defaultTemplateManager.Resolve(key);
            } else {
                return new LoadedTemplateSource(ctx.Source, ctx.FileName);
            }
        }
    }
}