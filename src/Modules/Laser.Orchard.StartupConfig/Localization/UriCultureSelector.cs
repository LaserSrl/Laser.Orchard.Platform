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

namespace Laser.Orchard.StartupConfig.Localization {

    [OrchardFeature("Laser.Orchard.StartupConfig.UriCultureSelector")]
    public class UriCultureSelector : ICultureSelector {

        private CultureSelectorResult _evaluatedResult;
        private bool _isEvaluated;


        public const int SELECTOR_PRIORITY = -1; //priority is higher than SiteCultureSelector priority (-5) and CookieCultureSelector(-2), But lower than ContentCulture(0) used by Orchard.CulturePicker

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
                requestPath = context.Request.AppRelativeCurrentExecutionFilePath.Replace("~", string.Empty);
                requestPath = (requestPath.StartsWith("/"))
                                                ? requestPath.Substring(1)
                                                : requestPath;
            } else {
                requestPath = context.Request.QueryString["lang"].ToString() + "/";
            }
            if (string.IsNullOrWhiteSpace(requestPath)) {
                return (null);
            }
            try {
                var cultureToken = requestPath.Substring(0, requestPath.IndexOf('/'));
                // Recupero la cultura indicata 
                var fullCultureName = CultureInfo.GetCultures(CultureTypes.AllCultures).
                    Where(w => w.Name.StartsWith(cultureToken)).Select(s => s.Name).FirstOrDefault();
                if (fullCultureName != null) {

                    var cultureInfo = CultureInfo.GetCultureInfo(fullCultureName);
                    return new CultureSelectorResult { Priority = SELECTOR_PRIORITY, CultureName = cultureInfo.Name };
                } else { return null; }
            } catch {
                return null;
            }
        }
    }
}