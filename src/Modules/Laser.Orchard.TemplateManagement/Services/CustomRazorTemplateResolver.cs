using Laser.Orchard.StartupConfig.Providers;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TemplateManagement.Services {
    public class CustomRazorTemplateResolver : ICustomRazorTemplateResolver {
        public string GetKeyName(string name, ResolveType resolveType, ITemplateKey context) {
            // Context object with original name, new name, resolverType, context, flag (name found)
            throw new NotImplementedException();
        }

        public string GetTemplateSource(ITemplateKey key) {
            // Context object with key, source and file name
            throw new NotImplementedException();
        }
    }
}