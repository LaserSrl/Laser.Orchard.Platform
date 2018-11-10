using System;
using System.Collections.Generic;
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
						AddAlternates(alternates, "Menu");
						break;
					case "MenuItem":
						AddAlternates(alternates, "MenuItem");
						break;
					case "MenuItemLink":
						AddAlternates(alternates, "MenuItemLink");
						break;
				}
			});
		}

		private static void AddAlternates(IList<String> alternateCollection, String shapeName) {
			alternateCollection.Add(shapeName);
			//alternateCollection.Add(shapeName + "__" + contentTypeName + "__" + zoneName);
		}
	}
}