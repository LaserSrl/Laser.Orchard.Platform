using System;
using System.Web;
using System.Web.Routing;
using Orchard;
using Orchard.Alias;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Configuration;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;
using Orchard.UI.Admin;
using Orchard.Autoroute.Models;

namespace Laser.Orchard.CulturePicker.Services {
    public class ContentCultureSelector : ICultureSelector {
        public const int SelectorPriority = 0; //priority is higher than SiteCultureSelector priority (-5)
        private readonly IAliasService _aliasService;
        private readonly IOrchardServices _orchardServices;
        private readonly ICulturePickerServices _cpServices;

        private CultureSelectorResult _evaluatedResult;
        private bool _isEvaluated;

        public ContentCultureSelector(IAliasService aliasService, IOrchardServices orchardServices, ICulturePickerServices cpServices) {
            _aliasService = aliasService;
            _orchardServices = orchardServices;
            _cpServices = cpServices;
        }

        #region ICultureSelector Members

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
            if (context == null || context.Request == null) {
                return null;
            }

            if (AdminFilter.IsApplied(context.Request.RequestContext)) { // I am in admin context so I have to use defualt site culture
                return new CultureSelectorResult { Priority = SelectorPriority, CultureName = _orchardServices.WorkContext.CurrentSite.SiteCulture };
            }

            //TODO: check for a more efficient way to get content item for the current request
            string relativePath = Utils.GetAppRelativePath(context.Request.Url.AbsolutePath, context.Request);
            var urlPrefix = _orchardServices.WorkContext.Resolve<ShellSettings>().RequestUrlPrefix;
            if (!String.IsNullOrWhiteSpace(urlPrefix)) {
                relativePath = relativePath.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase) ? relativePath.Substring(urlPrefix.Length) : relativePath;
            }
            relativePath = relativePath.StartsWith("/") ? relativePath.Substring(1) : relativePath;
            relativePath = HttpUtility.UrlDecode(relativePath);

            if (context.Request != null && context.Request.RequestContext.RouteData != null && context.Request.RequestContext.RouteData.Values["controller"] != null) {
                if (context.Request.RequestContext.RouteData.Values["controller"].ToString().ToLower() == "blogpost" && relativePath.IndexOf("/archive") > -1) {
                    relativePath = relativePath.Substring(0, relativePath.IndexOf("/archive")); // prendo sull'url che definisce il blog
                }
            }
            RouteValueDictionary routeValueDictionary = _aliasService.Get(relativePath);

            if (routeValueDictionary == null) {
                return null;
            }

            int routeId = Convert.ToInt32(routeValueDictionary["Id"]); //Convert.ToInt32(null) == 0
            if (routeId == 0) {
                routeId = Convert.ToInt32(routeValueDictionary["blogId"]); //forse è un blog?
            }

            ContentItem content = _orchardServices.ContentManager.Get(routeId, VersionOptions.Published);
            if (content == null) {
                return null;
            }

            //NOTE: we can't use ILocalizationService.GetContentCulture for this, because it causes circular dependency
            var localized = content.As<ILocalizableAspect>();
            string currentCultureName = "";
            if (localized == null) {
                var term = content.As<TermPart>();
                if (term == null) {
                    return null; // non ha la localization part e non ha nemmeno una tassonomia tradotta
                } else {
                    // verifico se per caso sto visualizzando una termPart che è figlia di una tassonomia tradottta: in tal caso la culture deve essere quella della tassonomia
                    localized = _orchardServices.ContentManager.Get(term.TaxonomyId).As<ILocalizableAspect>();
                    if (localized == null) {
                        return null;
                    } else {
                        currentCultureName = localized.Culture;
                    }
                }
            } else if (string.IsNullOrEmpty(localized.Culture)) {
                // ha la localization part, ma non è tradotta => prendo la site culture
                currentCultureName = _orchardServices.WorkContext.CurrentSite.SiteCulture;
            } else {
                currentCultureName = localized.Culture;
            }

            _cpServices.SaveCultureCookie(currentCultureName, context);
            return new CultureSelectorResult { Priority = SelectorPriority, CultureName = currentCultureName };
        }

        #endregion
    }
}