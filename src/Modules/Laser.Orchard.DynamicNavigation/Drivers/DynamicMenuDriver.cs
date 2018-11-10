using Laser.Orchard.DynamicNavigation.Helpers;
using Laser.Orchard.DynamicNavigation.Models;
using Laser.Orchard.DynamicNavigation.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Services;
using Orchard.UI.Navigation;
using Orchard.Localization;
using Orchard.Core.Title.Models;
using System.Collections.Generic;
using Orchard.Utility.Extensions;
using System;
using System.Linq;
using Orchard.ContentManagement.Handlers;


namespace Laser.Orchard.DynamicNavigation.Drivers {


    public class DynamicMenuDriver : ContentPartCloningDriver<DynamicMenuPart> {


        private readonly IContentManager _contentManager;
        private readonly INavigationManager _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IMenuService _menuService;


        public DynamicMenuDriver(IContentManager contentManager,
            INavigationManager navigationManager,
            IWorkContextAccessor workContextAccessor,
            IMenuService menuService) {

            _contentManager = contentManager;
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
            _menuService = menuService;
            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }


        protected override string Prefix {
            get { return "DynamicMenu"; }
        }


        protected override DriverResult Display(DynamicMenuPart part, string displayType, dynamic shapeHelper) {

            var menu = _menuService.GetMenu(part.MenuId);
            var menuName = menu.As<TitlePart>().Title.HtmlClassify();
            var currentCulture = _workContextAccessor.GetContext().CurrentCulture;

            IEnumerable<MenuItem> menuItems = _navigationManager.BuildMenu(menu);

            var localized = new List<MenuItem>();
            foreach (var menuItem in menuItems) {
                // if there is no associated content, it as culture neutral
                if (menuItem.Content == null) {
                    localized.Add(menuItem);
                }

                // if the menu item is culture neutral or of the current culture
                else if (String.IsNullOrEmpty(menuItem.Culture) || String.Equals(menuItem.Culture, currentCulture, StringComparison.OrdinalIgnoreCase)) {
                    localized.Add(menuItem);
                }
            }

            menuItems = localized;

            var routeData = _workContextAccessor.GetContext().HttpContext.Request.RequestContext.RouteData;


            // ## DA VERIFICARE ##
            //  06/08/2013 - Modificato da max per far compilare la soluzione modificata con la versione ORCHARD 1.7
            var selectedPath = NavigationHelper.SetSelectedPath(menuItems, _workContextAccessor.GetContext().HttpContext.Request, routeData);

            // var selectedPath = NavigationHelper.SetSelectedPath(menuItems, routeData);

            dynamic menuShape = shapeHelper.DynamicMenu();
            IEnumerable<MenuItem> filteredMenuItems = selectedPath;
            // apply level limits to Dynamic Navigation (supported 3 levels)
            if (filteredMenuItems != null) {
                var numberOfItems = filteredMenuItems != null ? filteredMenuItems.Count() : 0;
                var currentItemStartLevel = (numberOfItems >= part.LevelsToShow) ? numberOfItems - part.LevelsToShow : 0;
                if (filteredMenuItems != null && filteredMenuItems.Last().Items != null && filteredMenuItems.Last().Items.Count() > 0 && numberOfItems >= part.LevelsToShow) {
                    currentItemStartLevel += 1;
                }
                filteredMenuItems = filteredMenuItems.Skip(currentItemStartLevel);
                var rootMenuItem = filteredMenuItems.Take(1);
                var hyerarchicalItems = DynamicNavigationHelper.GetSelectedItemsHierarchy(rootMenuItem, part.LevelsToShow);
                if (part.ShowFirstLevelBrothers) { // devo recuperare i fratelli del nodo 0 del dynamic menu
                    var splittedPositions = rootMenuItem.Single().Position.Split('.');
                    var nodeLevel = splittedPositions.Length - 1;
                    IEnumerable<MenuItem> itemsToScan = menuItems;
                    var incrementalPosition = "";
                    for (var i = 0; i <= nodeLevel; i++) {
                        if (i == nodeLevel) {
                            hyerarchicalItems = itemsToScan;
                        } else {
                            incrementalPosition += "." + splittedPositions[i].ToString();
                            if (incrementalPosition.StartsWith(".")) {
                                incrementalPosition = incrementalPosition.Substring(1);
                            }
                            itemsToScan = itemsToScan.Where(w => w.Position == incrementalPosition).FirstOrDefault().Items;
                        }

                    }


                }
                menuShape.MenuName(menuName);
                DynamicNavigationHelper.PopulateDynamicMenu(shapeHelper, menuShape, menuShape, hyerarchicalItems);

                return ContentShape("Parts_DynamicMenu",
                  () => shapeHelper.Parts_DynamicMenu(Menu: menuShape));
            } else {
                return null;
            }
        }


        //GET
        protected override DriverResult Editor(DynamicMenuPart part, dynamic shapeHelper) {

            var viewModelPart = new DynamicMenuViewModel {
                MenuId = part.MenuId,
                LevelsToShow = part.LevelsToShow,
                ShowFirstLevelBrothers = part.ShowFirstLevelBrothers,
                Menus = _menuService.GetMenus()
            };

            return ContentShape("Parts_DynamicMenu_Edit",
              () => shapeHelper.EditorTemplate(
                  TemplateName: "Parts/DynamicMenu",
                  Model: viewModelPart,
                  Prefix: Prefix
                ));
        }


        //POST
        protected override DriverResult Editor(DynamicMenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new DynamicMenuViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                part.LevelsToShow = model.LevelsToShow;
                part.MenuId = model.MenuId;
                part.ShowFirstLevelBrothers = model.ShowFirstLevelBrothers;
                //updater.TryUpdateModel(part, Prefix, null, null);
            }
            return Editor(part, shapeHelper);
        }

        protected override void Cloning(DynamicMenuPart originalPart, DynamicMenuPart clonePart, CloneContentContext context) {
            clonePart.MenuId = originalPart.MenuId;
            clonePart.LevelsToShow = originalPart.LevelsToShow;
            clonePart.ShowFirstLevelBrothers = originalPart.ShowFirstLevelBrothers;
        }
        //protected override void Importing(DynamicMenuPart part, ImportContentContext context) {
            
        //    //mod 05-12-2016
        //    context.ImportAttribute(part.PartDefinition.Name, "MenuId", x => {
        //        var tempPartFromid = context.GetItemFromSession(x);

        //        if (tempPartFromid != null && tempPartFromid.Is<MenuPart>()) {
        //            //associa id menu
        //            part.MenuId = tempPartFromid.As<MenuPart>().Id;
        //        }
        //    });


        //    var importedLevelsToShow = context.Attribute(part.PartDefinition.Name, "LevelsToShow");
        //    if (importedLevelsToShow != null) {
        //        part.LevelsToShow = int.Parse(importedLevelsToShow);
        //    }

        //    var importedShowFirstLevelBrothers = context.Attribute(part.PartDefinition.Name, "ShowFirstLevelBrothers");
        //    if (importedShowFirstLevelBrothers != null) {
        //        part.ShowFirstLevelBrothers = bool.Parse(importedShowFirstLevelBrothers);
        //    }

        //}

        //protected override void Exporting(DynamicMenuPart part, ExportContentContext context) {
            
        //    //mod. 05-12-2016
        //    var root = context.Element(part.PartDefinition.Name);
            
        //    if (part.MenuId > 0) {
        //        //cerco il corrispondente valore dell' identity dalla parts del menu e lo associo al campo menuid 
        //        var contItemMenu = _contentManager.Get(part.MenuId);
        //        if (contItemMenu != null) {
        //            root.SetAttributeValue("MenuId", _contentManager.GetItemMetadata(contItemMenu).Identity.ToString());
        //        }
              
        //    }
        //    root.SetAttributeValue("LevelsToShow", part.LevelsToShow);
        //    root.SetAttributeValue("ShowFirstLevelBrothers", part.ShowFirstLevelBrothers);
        //}

    }
}