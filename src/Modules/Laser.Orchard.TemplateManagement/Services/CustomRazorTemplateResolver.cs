using Laser.Orchard.StartupConfig.RazorBase.Models;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Laser.Orchard.TemplateManagement.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.TemplateManagement.Services {
    public class CustomRazorTemplateResolver : ICustomRazorTemplateResolver {
        private readonly IOrchardServices _orchardServices;
        private IDictionary<string, TemplatePart> _resolvedParts;

        public int Priority {
            get {
                return 20;
            }
        }

        public CustomRazorTemplateResolver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _resolvedParts = new Dictionary<string, TemplatePart>();
        }

        private TemplatePart GetTemplate(string templateCode) {
            // I use a dictionary to save the template I find.
            //This way I avoid executing the same query twice for each request (both in GetKeyName and ResolveTemplateSource).
            if (!_resolvedParts.ContainsKey(templateCode)) {
                var template = _orchardServices
                   .ContentManager
                   .Query<TemplatePart, TemplatePartRecord>(VersionOptions.Published)
                   .Where(tpr => tpr.TemplateCode == templateCode)
                   .Slice(0, 1)
                   .FirstOrDefault();

                _resolvedParts.Add(templateCode, template);
            }
            return _resolvedParts[templateCode];
        }

        public void GetKeyName(KeyNameResolveContext context) {
            if (context.ResolveType == ResolveType.Include) {
                var name = context.OriginalName;

                if (!name.EndsWith(".cshtml") && name.Contains(".cshtml")) {
                    name = name.Substring(0, name.LastIndexOf(".cshtml"));
                }

                var customTemplate = GetTemplate(name);
                if (customTemplate != null) {
                    // Adding Version to the key to make it unique in cache.
                    name += ":Version" + customTemplate.ContentItem.VersionRecord.Id.ToString();
                    context.NameResolved = true;
                    context.ResolvedName = name;
                }
            }
        }

        public void ResolveTemplateSource(TemplateSourceResolveContext context) {
            if (string.IsNullOrWhiteSpace(context.FileName)) {
                return;
            }

            var fileName = context.FileName;

            if (context.Key.TemplateType == ResolveType.Include) {
                fileName = fileName.Substring(0, fileName.LastIndexOf(":Version"));
            }

            // I need to search for the key into the CustomTemplates.
            var customTemplate = GetTemplate(fileName);
            if (customTemplate != null) {
                context.Source = customTemplate.Text;
                fileName += customTemplate.ContentItem.VersionRecord.Id.ToString();
                context.FileName = fileName + ".cshtml";
            }
        }
    }
}