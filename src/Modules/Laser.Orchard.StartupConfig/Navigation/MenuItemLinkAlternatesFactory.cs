using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using System.Linq;

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
                        InsertAlternate(alternates, "Menu");
                        break;
					case "MenuItem":
                        InsertAlternate(alternates, "MenuItem");
                        break;
					case "MenuItemLink":
                        InsertAlternate(alternates, "MenuItemLink");
						break;
				}
			});
		}

        /// <summary>
        /// Adds the alternate inside the alternates list before any specific alternate, based on ShapeName.
        /// </summary>
        /// <param name="alternateCollection"></param>
        /// <param name="ShapeName"></param>
        private static void InsertAlternate(IList<string> alternateCollection, string ShapeName) {
            var index = 0;
            var firstShape = alternateCollection.FirstOrDefault(a => a.StartsWith(ShapeName));
            if (firstShape != null && alternateCollection.IndexOf(firstShape) > 0) {
                index = alternateCollection.IndexOf(firstShape);
            }
            alternateCollection.Insert(index, ShapeName);
        }
    }
}