using System;
using System.Web;
using Orchard;
using Orchard.Localization.Services;
using Orchard.UI.Admin;

namespace Laser.Orchard.CulturePicker.Services {
    public class CookieCultureSelector : ICultureSelector {
        private readonly IOrchardServices _orchardServices;
        private readonly ICulturePickerServices _cpServices;
        public const string CultureCookieName = "cultureData";
        public const string CurrentCultureFieldName = "currentCulture";
        public const int SelectorPriority = -2; //priority is higher than SiteCultureSelector priority (-5), But lower than ContentCultureSelector (0) 

        private CultureSelectorResult _evaluatedResult;
        private bool _isEvaluated;

        #region ICultureSelector Members
        public CookieCultureSelector(
            IOrchardServices orchardServices,
            ICulturePickerServices cpServices) {

            _orchardServices = orchardServices;
            _cpServices = cpServices;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!_isEvaluated) {
                _isEvaluated = true;
                _evaluatedResult = EvaluateResult(context);
            }

            return _evaluatedResult;
        }

        #endregion

        #region Helpers

        private CultureSelectorResult EvaluateResult(HttpContextBase context) {

            if (context == null || context.Request == null || context.Request.Cookies == null) {
                return null;
            }
            if (AdminFilter.IsApplied(context.Request.RequestContext)) { // I am in admin context so I have to use defualt site culture
                return new CultureSelectorResult { Priority = SelectorPriority, CultureName = _orchardServices.WorkContext.CurrentSite.SiteCulture };
            }

            HttpCookie cultureCookie = context.Request.Cookies[context.Request.AnonymousID + CultureCookieName];
            if (cultureCookie == null) {
                return null;
            }

            string currentCultureName = cultureCookie[CurrentCultureFieldName];
            if (String.IsNullOrEmpty(currentCultureName)) {
                return null;
            }
            return new CultureSelectorResult { Priority = SelectorPriority, CultureName = currentCultureName };
        }

        private bool UseSubdomainCookie(HttpContextBase context) {
            //TODO: write an actual implementation based, e.g., on a site setting
            return false;
        }

        #endregion
    }
}