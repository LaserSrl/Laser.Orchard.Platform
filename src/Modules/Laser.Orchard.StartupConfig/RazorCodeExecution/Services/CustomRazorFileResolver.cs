using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.RazorBase.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {
    public class CustomRazorFileResolver : ICustomRazorTemplateResolver {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private string[] _fallbackTenants;

        public int Priority {
            get {
                return 10;
            }
        }

        private string[] GetFallbackTenants() {
            if (!_fallbackTenants.Any()) {
                var envVariablesSettingsPart = _orchardServices.WorkContext.CurrentSite.As<EnvironmentVariablesSettingsPart>();
                if (envVariablesSettingsPart != null) {
                    if (string.IsNullOrWhiteSpace(envVariablesSettingsPart.FallbackTenants) == false) {
                        _fallbackTenants = envVariablesSettingsPart.FallbackTenants.Split(';');
                    }
                }
            }
            return _fallbackTenants;
        }

        public CustomRazorFileResolver(IOrchardServices orchardServices,
            ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;

            // Fallback tenants
            _fallbackTenants = new string[] { };

        }

        public void GetKeyName(KeyNameResolveContext context) {
            if (context.ResolveType == ResolveType.Include) {
                var uriDir = string.Empty;
                var templatePath = string.Empty;
                var name = context.OriginalName;

                if (!name.EndsWith(".cshtml") && name.Contains(".cshtml")) {
                    name = name.Substring(0, name.LastIndexOf(".cshtml"));
                }

                // I look into the Site folder for Code.
                uriDir = string.Format("~/App_Data/Sites/{0}/{1}", _shellSettings.Name, "Code");
                templatePath = Path.Combine(uriDir, name + ".cshtml");
                if (templatePath.StartsWith("~")) {
                    templatePath = HostingEnvironment.MapPath(templatePath);
                }
                if (File.Exists(templatePath)) {
                    DateTime d = System.IO.File.GetLastWriteTime(templatePath);
                    name += d.ToShortDateString() + d.ToLongTimeString();
                    context.NameResolved = true;
                    context.ResolvedName = name;
                }

                // Now I need to look into the fallback tenants
                if (!context.NameResolved) {
                    foreach (var ft in GetFallbackTenants()) {
                        if (!context.NameResolved) {
                            uriDir = string.Format("~/App_Data/Sites/{0}/{1}", ft, "Code");
                            templatePath = Path.Combine(uriDir, name + ".cshtml");
                            if (templatePath.StartsWith("~")) {
                                templatePath = HostingEnvironment.MapPath(templatePath);
                            }
                            if (File.Exists(templatePath)) {
                                DateTime d = System.IO.File.GetLastWriteTime(templatePath);
                                name += d.ToShortDateString() + d.ToLongTimeString();
                                context.NameResolved = true;
                                context.ResolvedName = name;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void ResolveTemplateSource(TemplateSourceResolveContext context) {
            if (string.IsNullOrWhiteSpace(context.FileName)) {
                return;
            }

            var fileName = context.FileName;

            if (!fileName.EndsWith(".cshtml") && fileName.Contains(".cshtml")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf(".cshtml"));
            }

            fileName += ".cshtml";

            var uriDir = string.Empty;
            var templatePath = string.Empty;

            // I look into the Site folder for Code.
            uriDir = string.Format("~/App_Data/Sites/{0}/{1}", _shellSettings.Name, "Code");
            templatePath = Path.Combine(uriDir, fileName);
            if (templatePath.StartsWith("~")) {
                templatePath = HostingEnvironment.MapPath(templatePath);
            }
            if (File.Exists(templatePath)) {
                context.Source = File.ReadAllText(templatePath);
                context.FileName = fileName;
            }

            // Now I need to look into the fallback tenants
            if (string.IsNullOrWhiteSpace(context.Source)) {
                foreach (var ft in GetFallbackTenants()) {
                    if (string.IsNullOrWhiteSpace(context.Source)) {
                        uriDir = string.Format("~/App_Data/Sites/{0}/{1}", ft, "Code");
                        templatePath = Path.Combine(uriDir, fileName);
                        if (templatePath.StartsWith("~")) {
                            templatePath = HostingEnvironment.MapPath(templatePath);
                        }
                        if (File.Exists(templatePath)) {
                            context.Source = File.ReadAllText(templatePath);
                            context.FileName = fileName;
                            break;
                        }
                    }
                }
            }
        }
    }
}