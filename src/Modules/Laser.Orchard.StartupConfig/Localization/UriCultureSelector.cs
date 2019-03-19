using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.StartupConfig.Localization {

    [OrchardFeature("Laser.Orchard.StartupConfig.UriCultureSelector")]
    public class UriCultureSelector : ICultureSelector {
        private readonly ShellSettings _shellSettings;
        private readonly ICultureManager _cultureManager;

        public UriCultureSelector(
            ShellSettings shellSettings,
            ICultureManager cultureManager) {

            _shellSettings = shellSettings;
            _cultureManager = cultureManager;
        }

        private CultureSelectorResult _evaluatedResult;
        private bool _isEvaluated;

        // priority is higher than SiteCultureSelector priority (-5) and 
        // CookieCultureSelector(-2), but lower than ContentCulture(0) used 
        // by Orchard.CulturePicker
        public const int SELECTOR_PRIORITY = -1;

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!_isEvaluated) {
                _isEvaluated = true;
                _evaluatedResult = EvaluateResult(context);
            }

            return _evaluatedResult;
        }

        private CultureSelectorResult EvaluateResult(HttpContextBase context) {
            if (context == null) {
                return null;
            }

            string requestPath = "";
            if (context.Request.QueryString["lang"] == null) {
                requestPath = context.Request
                    .AppRelativeCurrentExecutionFilePath.Replace("~", string.Empty);

                requestPath = (requestPath.StartsWith("/"))
                    ? requestPath.Substring(1)
                    : requestPath;
                if (!string.IsNullOrWhiteSpace(UrlPrefix) && requestPath.StartsWith(UrlPrefix)) {
                    requestPath = requestPath.Substring(UrlPrefix.Length);
                }
            } else {
                requestPath = context.Request.QueryString["lang"].ToString() + "/";
            }
            if (string.IsNullOrWhiteSpace(requestPath)) {
                return (null);
            }
            try {
                var cultureToken = requestPath.Substring(0, requestPath.IndexOf('/'));
                if (!string.IsNullOrWhiteSpace(cultureToken)) { // sanity check
                    // cultureToken may not be the actual culture name. For example, it may be 'it'
                    // rather than 'it-IT'. This means we should not be using the list of CultureInfo
                    // objects in the system: there may be several cultures starting with the same
                    // string. 
                    // Rather, we should use the configured cultures in the Orchard application.
                    var cultures = _cultureManager
                        .ListCultures(); // list all configured cultures: these are the culture names
                    // first we see if a culture's name matches our token exactly
                    var cultureName = cultures
                        .FirstOrDefault(c => c.Equals(cultureToken, StringComparison.InvariantCultureIgnoreCase));
                    if (string.IsNullOrWhiteSpace(cultureName)) {
                        // lacking an exact match, we get the first culture for the same language.
                        cultureName = cultures
                            .FirstOrDefault(c => c.StartsWith(cultureToken, StringComparison.InvariantCultureIgnoreCase));
                    }
                    // cultureName here may not be the intended culture when the cultureToken does not 
                    // match a culture's name exactly.
                    if (!string.IsNullOrWhiteSpace(cultureName)) {
                        return new CultureSelectorResult { Priority = SELECTOR_PRIORITY, CultureName = cultureName };
                    }
                }
            } catch {
                return null;
            }
            return null;
        }

        private string UrlPrefix {
            get {
                var prefix = _shellSettings.RequestUrlPrefix;
                if (!string.IsNullOrWhiteSpace(prefix)) {
                    prefix += "/";
                }
                return prefix;
            }
        }
    }
}