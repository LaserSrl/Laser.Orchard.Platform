using Orchard.ContentManagement;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Localization.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Laser.Orchard.CulturePicker.Services {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemServices : ITranslateMenuItemsServices {

        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly IMenuService _menuService;
        private readonly IOrchardServices _orchardServices;

        public TranslateMenuItemServices(
            ILocalizationService localizationService, 
            IContentManager contentManager,
            IMenuService menuService,
            IOrchardServices orchardServices) {

            _localizationService = localizationService;
            _contentManager = contentManager;
            _menuService = menuService;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool TryTranslateAllSubmenus(TranslateMenuItemsPart part) {
            var menu = part.ContentItem;
            //get the Id of the translated menu
            int tMenuId = menu.Id;
            //get the target culture from the localization part in the menu
            var locPart = menu.As<LocalizationPart>(); //this contains the localization info
            string targetCulture = locPart.Culture.Culture;
            int targetCultureId = locPart.Culture.Id;
            //find the master menu item
            var locInfo = _localizationService.GetLocalizations(menu, VersionOptions.Published);
            int masterId = 0;
            foreach (var loc in locInfo) {
                if (loc.MasterContentItem == null) { //found master
                    masterId = loc.Id;
                    part.FromLocale = loc.Culture.Culture;
                    break;
                }
            }
            if (masterId == 0) {
                //the menu we are using as target form translation is the master, so we cannot really do anything
                return true;
                //going forward with masterId == 0 in orchard 1.10 actually crashes the navigation of the site
                //because _menuServicec.GetMenu(0) actually returns a menu that is weird.
            }
            var masterMenu = _menuService.GetMenu(masterId);//_contentManager.Get(masterId);
            var thisMenu = _menuService.GetMenu(tMenuId);

            var masterParts = _menuService.GetMenuParts(masterId);

            bool flawlessly = true; //set to false if there was some issue in the localization

            //duplicate the parts into the new menu
            foreach (var origPart in masterParts) {
                var menuPart = _contentManager.New<MenuPart>(origPart.ContentItem.ContentType);

                menuPart.MenuPosition = origPart.MenuPosition;
                menuPart.Menu = thisMenu;
                menuPart.MenuText = origPart.MenuText + " " + targetCulture;
  
                _contentManager.Create(menuPart);

                //This is horrible to behold:
                //For each type of MenuItem, we go and copy/localize the relevant fields.
                //We may have to restructure all MenuItems if we want to make this more flexible, so that they
                //contain information about the members to be copied over or localized.
                
                if (origPart.ContentItem.ContentType == "ContentMenuItem") {
                    //The content element may be localized
                    int ciId = ((dynamic)origPart.ContentItem).ContentMenuItemPart.ContentItemId;
                    int newCiId = ciId;
                    var origContent = _contentManager.Get(ciId);
                    if (origContent != null) {
                        var localizations = _localizationService.GetLocalizations(origContent, VersionOptions.Published);
                        foreach (var loc in localizations) {
                            if (loc.Culture.Id == targetCultureId) {
                                newCiId = loc.Id;
                                break;
                            }
                        }
                        if (newCiId == ciId) {
                            flawlessly = false;
                            _orchardServices.Notifier.Error(T("The content for Menu Item {0} should be localized.", origPart.MenuText));
                        }
                    } else {
                        flawlessly = false;
                        _orchardServices.Notifier.Error(T("The content for Menu Item {0} could not be found.", origPart.MenuText));
                    }
                    ((dynamic)menuPart.ContentItem).ContentMenuItemPart.ContentItemId = newCiId;
                } else if (origPart.ContentItem.ContentType == "MenuItem") {
                    ((dynamic)menuPart.ContentItem).MenuItemPart.Url = ((dynamic)origPart.ContentItem).MenuItemPart.Url;
                } else if (origPart.ContentItem.ContentType == "HtmlMenuItem") {
                    ((dynamic)menuPart.ContentItem).BodyPart.Text = ((dynamic)origPart.ContentItem).BodyPart.Text;
                    _orchardServices.Notifier.Warning(T("Verify the Url in Menu Item {0}.", origPart.MenuText));
                } else if (origPart.ContentItem.ContentType == "NavigationQueryMenuItem") {
                    ((dynamic)menuPart.ContentItem).NavigationQueryPart.QueryPartRecord = ((dynamic)origPart.ContentItem).NavigationQueryPart.QueryPartRecord;
                } else if (origPart.ContentItem.ContentType == "TaxonomyNavigationMenuItem") {
                    //the taxonomy may be localized
                    int tId = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.TaxonomyId;
                    int newTId = tId;
                    var origContent = _contentManager.Get(tId);
                    if (origContent != null) {
                        var localizations = _localizationService.GetLocalizations(origContent, VersionOptions.Published);
                        foreach (var loc in localizations) {
                            if (loc.Culture.Id == targetCultureId) {
                                newTId = loc.Id;
                                break;
                            }
                        }
                        if (newTId == tId) {
                            flawlessly = false;
                            _orchardServices.Notifier.Error(T("The content for Taxonomy Item {0} should be localized.", origPart.MenuText));
                        }
                    } else {
                        flawlessly = false;
                        _orchardServices.Notifier.Error(T("The content for Taxonomy Item {0} could not be found.", origPart.MenuText));
                    }
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.TaxonomyId = newTId;
                    //the "starting" term in this taxonomy may be localized
                    int termId = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.TermId;
                    int newTermId = termId;
                    if (termId != -1) { //termId == -1 => root term, whatever that is, so no translation required
                        var origTerm = _contentManager.Get(termId);
                        if (origTerm != null) {
                            var localizations = _localizationService.GetLocalizations(origTerm, VersionOptions.Published);
                            foreach (var loc in localizations) {
                                if (loc.Culture.Id == targetCultureId) {
                                    newTermId = loc.Id;
                                    break;
                                }
                            }
                            if (newTermId == termId) {
                                flawlessly = false;
                                _orchardServices.Notifier.Error(T("The content for a Term in Taxonomy Item {0} should be localized.", origPart.MenuText));
                            }
                        } else {
                            flawlessly = false;
                            _orchardServices.Notifier.Error(T("The content for a Term in Taxonomy Item {0} could not be found.", origPart.MenuText));
                        }
                    }
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.TermId = newTermId;
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.DisplayContentCount = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.DisplayContentCount;
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.DisplayRootTerm = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.DisplayRootTerm;
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.HideEmptyTerms = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.HideEmptyTerms;
                    ((dynamic)menuPart.ContentItem).TaxonomyNavigationPart.LevelsToDisplay = ((dynamic)origPart.ContentItem).TaxonomyNavigationPart.LevelsToDisplay;
                } else if (origPart.ContentItem.ContentType == "ShapeMenuItem") {
                    ((dynamic)menuPart.ContentItem).ShapeMenuItemPart.ShapeType = ((dynamic)origPart.ContentItem).ShapeMenuItemPart.ShapeType;
                }

            }

            if (flawlessly) {
                _orchardServices.Notifier.Information(T("The localization was successful."));
            }

            return true;
        }

    }
}
