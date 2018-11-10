using System.Collections.Generic;
using System.Linq;
using Orchard.UI.Navigation;


namespace Laser.Orchard.DynamicNavigation.Helpers {

  
  public static class DynamicNavigationHelper {


    public static IEnumerable<MenuItem> GetSelectedItemsHierarchy(IEnumerable<MenuItem> items, int levelsToShow) {
      var selectionDeep = 0;
      var resultItems = GetOnlySelectedHierarchy(items, ref selectionDeep);
      if (levelsToShow < selectionDeep)
        resultItems = resultItems.SelectMany(s => s.Items);
      return resultItems;
    }


    /// <summary>
    /// Populates the menu shapes.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="parentShape">The menu parent shape.</param>
    /// <param name="menu">The menu shape.</param>
    /// <param name="menuItems">The current level to populate.</param>
    public static void PopulateDynamicMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems) {
      foreach (MenuItem menuItem in menuItems) {
        dynamic menuItemShape = BuildDynamicMenuItemShape(shapeFactory, parentShape, menu, menuItem);

        if (menuItem.Items != null && menuItem.Items.Any()) {
          PopulateDynamicMenu(shapeFactory, menuItemShape, menu, menuItem.Items);
        }

        parentShape.Add(menuItemShape, menuItem.Position);
      }
    }


    /// <summary>
    /// Builds a menu item shape.
    /// </summary>
    /// <param name="shapeFactory">The shape factory.</param>
    /// <param name="parentShape">The parent shape.</param>
    /// <param name="menu">The menu shape.</param>
    /// <param name="menuItem">The menu item to build the shape for.</param>
    /// <returns>The menu item shape.</returns>
    public static dynamic BuildDynamicMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem) {
      var menuItemShape = shapeFactory.DynamicMenuItem()
          .Text(menuItem.Text)
          .IdHint(menuItem.IdHint)
          .Href(menuItem.Href)
          .LinkToFirstChild(menuItem.LinkToFirstChild)
          .LocalNav(menuItem.LocalNav)
          .Selected(menuItem.Selected)
          .RouteValues(menuItem.RouteValues)
          .Item(menuItem)
          .Menu(menu)
          .Parent(parentShape)
          .Content(menuItem.Content);

      foreach (var className in menuItem.Classes)
        menuItemShape.Classes.Add(className);

      return menuItemShape;
    }


    private static IEnumerable<MenuItem> GetOnlySelectedHierarchy(IEnumerable<MenuItem> items, ref int selectionDeep) {

      if (selectionDeep == 0)
        selectionDeep = 1;

      var collectedItems = Enumerable.Empty<MenuItem>();

      foreach (var item in items) {
        if (!item.Selected) {
          item.Items = Enumerable.Empty<MenuItem>();
        } else {
          collectedItems.ToList().Add(item);
          if (item.Items.Count() > 0) {
            selectionDeep++;
            GetOnlySelectedHierarchy(item.Items, ref selectionDeep);
          }
        }
      }

      return items;
    }


  }
}