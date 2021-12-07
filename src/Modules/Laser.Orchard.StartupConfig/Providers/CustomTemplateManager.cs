using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Environment.Configuration;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.StartupConfig.Providers {
    public class CustomTemplateManager : ITemplateManager {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource> _dynamicTemplates =
            new System.Collections.Concurrent.ConcurrentDictionary<ITemplateKey, ITemplateSource>();
        private readonly ITemplateManager _defaultTemplateManager;
        private string[] _fallbackTenants;

        public CustomTemplateManager(ShellSettings shellSettings,
            IOrchardServices orchardServices,
            ITemplateManager defaultTemplateManager) {
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _defaultTemplateManager = defaultTemplateManager;

            // Fallback tenants
            var envVariablesSettingsPart = _orchardServices.WorkContext.CurrentSite.As<EnvironmentVariablesSettingsPart>();
            _fallbackTenants = new string[] { };
            if (envVariablesSettingsPart != null) {
                if (string.IsNullOrWhiteSpace(envVariablesSettingsPart.FallbackTenants) == false) {
                    _fallbackTenants = envVariablesSettingsPart.FallbackTenants.Split(';');
                }
            }
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
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        public ITemplateSource Resolve(ITemplateKey key) {
            var fileName = key.Name;

            if (!fileName.EndsWith(".cshtml") && fileName.Contains(".cshtml")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf(".cshtml"));
            }

            var src = string.Empty;

            // I need to search for the key into the CustomTemplates.
            var customTemplate = _orchardServices.ContentManager.Query(VersionOptions.Published, "CustomTemplate")
                .List()
                .FirstOrDefault(ct =>
                    ct.Parts
                        .SelectMany(pa => pa.Fields)
                        .Any(f => f is TextField &&
                            f.Name == "TemplateCode" &&
                            ((TextField)f).Value == fileName));
            if (customTemplate != null) {
                var templatePart = customTemplate.Parts
                    .FirstOrDefault(pa => pa.PartDefinition.Name.Equals("TemplatePart", StringComparison.InvariantCultureIgnoreCase));
                if (templatePart != null) {
                    src = ((dynamic)templatePart).Text;
                }
            }

            var uriDir = string.Empty;
            var templatePath = string.Empty;

            // I look into the Site folder for Code.
            if (string.IsNullOrWhiteSpace(src)) {
                uriDir = string.Format("~/App_Data/Sites/{0}/{1}", _shellSettings.Name, "Code");
                templatePath = Path.Combine(uriDir, fileName + ".cshtml");
                if (templatePath.StartsWith("~")) {
                    templatePath = HostingEnvironment.MapPath(templatePath);
                }
                if (File.Exists(templatePath)) {
                    src = File.ReadAllText(templatePath);
                }
            }
            
            // Now I need to look into the fallback tenants
            if (string.IsNullOrWhiteSpace(src)) {
                foreach(var ft in _fallbackTenants) {
                    if (string.IsNullOrWhiteSpace(src)) {
                        uriDir = string.Format("~/App_Data/Sites/{0}/{1}", ft, "Code");
                        templatePath = Path.Combine(uriDir, fileName + ".cshtml");
                        if (templatePath.StartsWith("~")) {
                            templatePath = HostingEnvironment.MapPath(templatePath);
                        }
                        if (File.Exists(templatePath)) {
                            src = File.ReadAllText(templatePath);
                            break;
                        }
                    }
                }
            }
            

            if (string.IsNullOrWhiteSpace(src)) {
                ITemplateSource result;
                if (_dynamicTemplates.TryGetValue(key, out result)) {
                    return result;
                }
                return _defaultTemplateManager.Resolve(key);
            } else {
                return new LoadedTemplateSource(src, fileName + ".cshtml");
            }
        }
    }
}