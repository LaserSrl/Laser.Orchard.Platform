using RazorEngine.Templating;
using System;

namespace Laser.Orchard.StartupConfig.Providers {
    public class CustomRazorCachingProvider : DefaultCachingProvider, ICachingProvider {
        public CustomRazorCachingProvider() : this(null) {

        }

        public CustomRazorCachingProvider(Action<string> registerForCleanup) : base(registerForCleanup) {

        }

        public new bool TryRetrieveTemplate(ITemplateKey templateKey, Type modelType, out ICompiledTemplate compiledTemplate) {
            var returnValue = base.TryRetrieveTemplate(templateKey, modelType, out compiledTemplate);
            return returnValue;
        }
    }
}