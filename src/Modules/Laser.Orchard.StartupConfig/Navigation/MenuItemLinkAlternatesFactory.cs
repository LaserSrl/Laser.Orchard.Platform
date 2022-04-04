using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;

namespace Laser.Orchard.StartupConfig.Navigation
{
	public class MenuItemLinkAlternatesFactory : ShapeDisplayEvents
	{
		public override void Displaying(ShapeDisplayingContext context) {
			context.ShapeMetadata.OnDisplaying(displayedContext => {
				var alternates = displayedContext.ShapeMetadata.Alternates;
				switch (displayedContext.ShapeMetadata.Type) {
					case "Parts_MenuWidget":
						var zoneName = displayedContext.Shape.ContentItem.WidgetPart.Zone;
						displayedContext.Shape.Menu.Zone = zoneName;
						break;
					case "Menu":
                        AddMenuAlternates(displayedContext);
                        break;
					case "MenuItem":
                        AddMenuItemAlternates(displayedContext);
                        break;
					case "MenuItemLink":
                        AddMenuItemLinkAlternates(displayedContext);
                        break;
				}
			});
		}

        private static void AddMenuAlternates(ShapeDisplayingContext displayedContext) {
            var alternates = displayedContext.ShapeMetadata.Alternates;
            var menu = displayedContext.Shape;
            string menuName = menu.MenuName;
            AddAlternates(alternates, "SimpleMenu");
            AddAlternates(alternates, "SimpleMenu__" + EncodeAlternateElement(menuName));
        }

        private static void AddMenuItemAlternates(ShapeDisplayingContext displayedContext) {
            var alternates = displayedContext.ShapeMetadata.Alternates;
            var menuItem = displayedContext.Shape;
            var menu = menuItem.Menu;
            int level = menuItem.Level;
            AddAlternates(alternates, "SimpleMenuItem");
            AddAlternates(alternates, "SimpleMenuItem__" + EncodeAlternateElement(menu.MenuName));
            AddAlternates(alternates, "SimpleMenuItem__" + EncodeAlternateElement(menu.MenuName) + "__level__" + level);
        }

        private static void AddMenuItemLinkAlternates(ShapeDisplayingContext displayedContext) {
            var alternates = displayedContext.ShapeMetadata.Alternates;
            var menuItem = displayedContext.Shape;
            string menuName = menuItem.Menu.MenuName;
            int level = menuItem.Level;
            string contentType = null;
            if (menuItem.Content != null) {
                contentType = ((IContent)menuItem.Content).ContentItem.ContentType;
            }
            if (contentType != null) {
                if (!string.Equals("HtmlMenuItem", contentType, StringComparison.InvariantCultureIgnoreCase)
                    && !string.Equals("ShapeMenuItem", contentType, StringComparison.InvariantCultureIgnoreCase)) {
                    // For HtmlMenuItems and ShapeMenuItems we are fine. We want to have a single alternate to fall back
                    // on for all those types of menu items that are conceptually just a link to a page, such as 
                    // "CustomLink" and "Content Menu Item" MenuItems.
                    AddAlternates(alternates, "SimpleMenuItemLink");
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(contentType));
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName));
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                    AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
                }
            } else {
                AddAlternates(alternates, "SimpleMenuItemLink");
                AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName));
                AddAlternates(alternates, "SimpleMenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);
            }
        }

        private static void AddAlternates(IList<String> alternateCollection, String shapeName) {
			alternateCollection.Add(shapeName);
			//alternateCollection.Add(shapeName + "__" + contentTypeName + "__" + zoneName);
		}

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames 
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private static string EncodeAlternateElement(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }
    }
}