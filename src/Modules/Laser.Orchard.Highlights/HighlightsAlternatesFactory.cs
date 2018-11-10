using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using System.Linq;
using Laser.Orchard.Highlights.Services;

namespace Laser.Orchard.Highlights {


    public class HighlightsAlternatesFactory : IShapeTableProvider {
        private readonly IHighlightsService _highlightsService;

        public HighlightsAlternatesFactory(IHighlightsService highlightsService) {
            _highlightsService = highlightsService;
        }

        public void Discover(ShapeTableBuilder builder) {
            var availablePlugins = _highlightsService.GetAvailablePlugins();
            builder.Describe("Parts_Banner")
                   .OnDisplaying(displaying => {
                       var shapeName = displaying.ShapeMetadata.Type;
                       var shapePlugin = displaying.Shape.HighlightsGroup.DisplayPlugin;
                       var shapeTemplate = displaying.Shape.HighlightsGroup.DisplayTemplate;
                       if (shapePlugin != "" && shapePlugin != "(default)") {
                           if (shapePlugin.StartsWith(shapeTemplate.ToString() + " - ")) {
                               var plugin = shapePlugin.Replace(shapeTemplate.ToString() + " - ", "").Trim().Replace(" ", "");
                               if (plugin != "") {
                                   displaying.Shape.Metadata.Alternates.Add(shapeName + "__" + plugin);
                               }
                           }
                       }
                   });
            builder.Describe("Parts_SlideShow")
                   .OnDisplaying(displaying => {
                       var shapeName = displaying.ShapeMetadata.Type;
                       var shapePlugin = displaying.Shape.HighlightsGroup.DisplayPlugin;
                       var shapeTemplate = displaying.Shape.HighlightsGroup.DisplayTemplate;
                       if (shapePlugin != "" && shapePlugin != "(default)") {
                           if (shapePlugin.StartsWith(shapeTemplate.ToString() + " - ")) {
                               var plugin = shapePlugin.Replace(shapeTemplate.ToString() + " - ", "").Trim().Replace(" ", "");
                               if (plugin != "") {
                                   displaying.Shape.Metadata.Alternates.Add(shapeName + "__" + plugin);
                               }
                           }
                       }
                   });
            builder.Describe("Parts_HighlightsItem")
                   .OnDisplaying(displaying => {
                       var shapeName = displaying.ShapeMetadata.Type;
                       var shape = displaying.Shape;
                       shape.Metadata.Alternates.Add(shapeName + "__" + displaying.ShapeMetadata.DisplayType);
                       var shapePlugin = displaying.Shape.HighlightsItem.GroupDisplayPlugin;
                       var shapeTemplate = displaying.Shape.HighlightsItem.GroupDisplayTemplate;
                       if (shapePlugin != "" && shapePlugin != "(default)") {
                           if (shapePlugin.StartsWith(shapeTemplate.ToString() + " - ")) {
                               var plugin = shapePlugin.Replace(shapeTemplate.ToString() + " - ", "").Trim().Replace(" ", "");
                               if (plugin != "") {
                                   shape.Metadata.Alternates.Add(shapeName + "__" + displaying.ShapeMetadata.DisplayType + "__" + plugin);
                                   shape.Metadata.Alternates.Add(shapeName + "__" + plugin);
                               }
                           }
                       }

                       //if (!string.IsNullOrEmpty(groupShapeName)) {
                       //    shape.Metadata.Alternates.Add((string)shapeName + "_" + EncodeAlternateElement(groupShapeName));
                       //    shape.Metadata.Alternates.Add((string)shapeName + "_" + EncodeAlternateElement(groupShapeName) + "__" + displaying.ShapeMetadata.DisplayType);
                       //}
                   });
            foreach (var item in availablePlugins) {
                builder.Describe("Parts_HighlightsItem_" + EncodeAlternateElement(item))
           .OnDisplaying(displaying => {
               var shapeName = displaying.ShapeMetadata.Type;
               var shape = displaying.Shape;
               var groupShapeName = shape.HighlightsItem.GroupShapeName;
               shape.Metadata.Alternates.Add(shapeName + "_" + displaying.ShapeMetadata.DisplayType);
           })
           ;


            }
        }

        private string EncodeAlternateElement(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }

    }
}