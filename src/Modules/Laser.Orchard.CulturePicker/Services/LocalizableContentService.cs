using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Laser.Orchard.CulturePicker.Models;
using System;
using Orchard.Alias;

namespace Laser.Orchard.CulturePicker.Services {


    public class LocalizableContentService : ILocalizableContentService {


        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly ILocalizationService _localizationService;
        private readonly ICulturePickerSettingsService _culturePickerSettingsService;
        private readonly IAliasService _aliasService;


        public LocalizableContentService(
            ILocalizationService localizationService, 
            ICultureManager cultureManager, 
            IContentManager contentManager, 
            ICulturePickerSettingsService culturePickerSettingsService,
            IAliasService aliasService) {

            _localizationService = localizationService;
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            _culturePickerSettingsService = culturePickerSettingsService;
            _aliasService = aliasService;
        }


        //Finds route part for the specified URL
        //Returns true if specified url corresponds to some content and route exists; otherwise - false

        #region ILocalizableContentService Members

        public bool TryGetRouteForUrl(string url, out AutoroutePart route) {
            if (string.IsNullOrWhiteSpace(url)) { //look for homepage
                var routeValues = _aliasService.Get(string.Empty);
                var homeCI = _contentManager.Get(int.Parse(routeValues["Id"].ToString()));

                route = homeCI.As<AutoroutePart>();
            } else {
                //first check for route (fast, case sensitive, not precise)
                route = _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .ForVersion(VersionOptions.Published)
                    .Where(r => r.DisplayAlias == url)
                    .List()
                    .FirstOrDefault();
            }

            return route != null;
        }


        //Finds localized route part for the specified content and culture
        //Returns true if localized url for content and culture exists; otherwise - false
        public bool TryFindLocalizedRoute(ContentItem routableContent, string cultureName, out AutoroutePart localizedRoute) {
            if (!routableContent.Parts.Any(p => p.Is<ILocalizableAspect>())) {
                localizedRoute = null;
                return false;
            }

            //var siteCulture = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture());
            var localizations = _localizationService.GetLocalizations(routableContent, VersionOptions.Published);

            ILocalizableAspect localizationPart = null, siteCultureLocalizationPart = null;
            foreach (var l in localizations) {

                if (l.Culture != null && l.Culture.Culture == cultureName) {
                    localizationPart = l;
                    break;
                }
                if (l.Culture == null && siteCultureLocalizationPart == null) {
                    siteCultureLocalizationPart = l;
                }
            }

            //try get localization part for default site culture
            if (localizationPart == null) {
                localizationPart = siteCultureLocalizationPart;
            }

            if (localizationPart == null) {
                localizedRoute = null;
                return false;
            }

            var localizedContentItem = localizationPart.ContentItem;
            localizedRoute = localizedContentItem.Parts.Single(p => p is AutoroutePart).As<AutoroutePart>();
            return true;
        }

        #endregion


        /// <summary>
        /// Returns a list of cultures that have a translation of url
        /// </summary>
        /// <param name="url">the url to discover for translated cultures</param>
        /// <returns>IList of Rich Cultures defined in Culture picker settings</returns>
        public IList<ExtendedCultureRecord> AvailableTranslations(string url, bool isHomePage = false) {
            var cultureList = new List<ExtendedCultureRecord>();
            if (isHomePage || !String.IsNullOrEmpty(url)) {
                AutoroutePart currentRoutePart;
                TryGetRouteForUrl(url, out currentRoutePart);
                if (currentRoutePart != null && currentRoutePart.As<LocalizationPart>() != null) {
                    try {
                        var localizations = _localizationService.GetLocalizations(currentRoutePart, VersionOptions.Published);
                        cultureList = _culturePickerSettingsService
                          .CultureList(localizations.Select(loc => (loc.Culture == null) ? "" : loc.Culture.Culture))
                          .Where(cl => cl.CultureCode.ToString(CultureInfo.InvariantCulture) != "").Distinct().ToList();
                    } catch { }
                }
            }
            return (cultureList);
        }

        /// <summary>
        /// Returns a boolean telling whether the ids provided correspond to the culture provided. If not,
        /// the out parameter contains the ids of the corresponding translated contents.
        /// </summary>
        /// <param name="originalIds">An <type>IList&ltint&gt</type> containing the Ids of the items we may have to translate.</param>
        /// <param name="cultureName">The string representation of the culture we are translating the content to.</param>
        /// <param name="translatedIds">An <type>IList&ltint&gt</type> containing the Ids of the translated items.</param>
        /// <returns><value>true</value> if the input ids ARE NOT in the culture given; <value>false</value> if the 
        /// input ids ARE in the culture given.</returns>
        public bool TryGetLocalizedId(IList<int> originalIds, string cultureName, out IList<int> translatedIds) {
            //var culture = _cultureManager.GetCultureByName(cultureName);
            translatedIds = new List<int>();
            bool redirect = false; //tells wheter there has been a translation of id
            foreach (int id in originalIds) {
                var item = _contentManager.Get(id);
                if (item.As<LocalizationPart>() != null) {
                    var locInfo = _localizationService.GetLocalizations(item, VersionOptions.Published); //AllVersions);
                    var tran = locInfo.Where(e => e.Culture.Culture == cultureName).FirstOrDefault();
                    if (tran == null)
                        translatedIds.Add(id);
                    else {
                        translatedIds.Add(tran.Id);
                        if (tran.Id != id)
                            redirect = true;
                    }
                }
            }
            return redirect;
        }

        /// <summary>
        /// Returns a boolean telling whether the is provided corresponds to the culture provided. If not,
        /// the out parameter contains the id of the corresponding translated contents.
        /// </summary>
        /// <param name="originalId">An <type>Iint</type> corresponding to the Ids of the item we may have to translate.</param>
        /// <param name="cultureName">The string representation of the culture we are translating the content to.</param>
        /// <param name="translatedIds">An <type>int</type> containing the Id of the translated item.</param>
        /// <returns><value>true</value> if the input id IS NOT in the culture given; <value>false</value> if the 
        /// input id IS in the culture given.</returns>
        public bool TryGetLocalizedId(int originalId, string cultureName, out int translatedIds) {
            //var culture = _cultureManager.GetCultureByName(cultureName);
            bool redirect = false; //tells wheter there has been a translation of id
            var item = _contentManager.Get(originalId);
            //var tran = _localizationService.GetLocalizedContentItem(item, cultureName); //use GetLocalizations
            var locInfo = _localizationService.GetLocalizations(item, VersionOptions.Published); //AllVersions);
            var tran = locInfo.Where(e => e.Culture.Culture == cultureName).FirstOrDefault();
            if (tran == null) //GetLocalizedContentItem returns null if the item is already localized
                translatedIds = originalId;
            else {
                translatedIds = tran.Id;
                if (tran.Id != originalId)
                    redirect = true;
            }
            return redirect;
        }
    }
}