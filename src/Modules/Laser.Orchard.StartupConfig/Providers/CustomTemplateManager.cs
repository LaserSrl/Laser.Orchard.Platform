using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Providers {
    public class CustomTemplateManager : ITemplateManager {
        private readonly ShellSettings _shellSettings;
        private readonly IContentManager _contentManager;
        
        public CustomTemplateManager(ShellSettings shellSettings,
            IContentManager contentManager) {
            _shellSettings = shellSettings;
            _contentManager = contentManager;
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source) {
            var name = key.Name;
            
            //throw new NotImplementedException("Dynamic templates are not supported");
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context) {
            
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        public ITemplateSource Resolve(ITemplateKey key) {
            var name = key.Name;

            var fileName = key.Name;

            if (!fileName.EndsWith(".cshtml") && fileName.Contains(".cshtml")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf(".cshtml"));
            }

            string src = string.Empty;
            // I need to search for the key into the CustomTemplates.
            


            // If I didn't find my template, I now need to look into the Site folder for Templates.
            var uriDir = string.Format("~/App_Data/Sites/{0}/{1}", _shellSettings.Name, "Templates");
            var templatePath = Path.Combine(uriDir, fileName + ".cshtml");
            if (string.IsNullOrWhiteSpace(src)) {
                if (File.Exists(templatePath)) {
                    src = File.ReadAllText(templatePath);
                }
            }

            // Now I look into the Site folder for Code.
            if (string.IsNullOrWhiteSpace(src)) {
                uriDir = string.Format("~/App_Data/Sites/{0}/{1}", _shellSettings.Name, "Code");
                templatePath = Path.Combine(uriDir, fileName + ".cshtml");
                if (string.IsNullOrWhiteSpace(src)) {
                    if (File.Exists(templatePath)) {
                        src = File.ReadAllText(templatePath);
                    }
                }
            }


            
            return new LoadedTemplateSource(src, fileName + ".cshtml");
            //throw new NotImplementedException();
        }
    }
}