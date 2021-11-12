using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Laser.Orchard.StartupConfig.ShortCodes;
using Orchard;
using Orchard.Logging;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.ShortCodes.Filters {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ShortCodesFilter : IHtmlFilter {
        private readonly IEnumerable<IShortCodeProvider> _providers;

        public ShortCodesFilter(IEnumerable<IShortCodeProvider> providers) {
            _providers = providers;
            Logger = NullLogger.Instance;
        }
        public ILogger Logger;
        public string ProcessContent(string text, string flavor) {
            var context = new EvaluateContext {
                SourceText = text
            };
            _providers.Invoke(x => {
                if (context.SourceText.IndexOf("[" + x.Describe().Signature.Trim()+" ") > -1) {
                    x.Evaluate(context);
                }
            }, Logger);
            return context.SourceText;
        }
    }
}