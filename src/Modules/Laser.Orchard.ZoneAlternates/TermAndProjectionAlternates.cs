using Orchard.DisplayManagement.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;
using Orchard.Projections.Models;

namespace Laser.Orchard.ZoneAlternates {
    public class TermAndProjectionAlternates : ShapeDisplayEvents {

        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
                .OnDisplaying(displayedContext => {
                    ContentItem contentItem = displayedContext.Shape.ContentItem;
                    ContentPart contentPart = displayedContext.Shape.ContentPart is ContentPart
                        ? displayedContext.Shape.ContentPart : null;

                    if (contentItem != null && contentPart != null) {
                        var displayType = displayedContext.ShapeMetadata.DisplayType;
                        var shapeName = displayedContext.ShapeMetadata.Type;

                        var andDisplayType = !string.IsNullOrWhiteSpace(displayType)
                            && !displayType.Equals("Detail");

                        var newShape = "";
                        var typeShape = "";

                        if (contentPart is TermPart) {
                            if (contentItem.Is<ProjectionPart>()) {
                                // [ShapeName]-WithProjection.cshtml
                                // [ShapeName]-[ContentTypeName]-WithProjection.cshtml
                                newShape = shapeName + "__WithProjection";
                                typeShape = shapeName + "__"
                                    + contentItem.ContentType + "__WithProjection";
                                
                            }
                        } else if (contentPart is ProjectionPart) {
                            if (contentItem.Is<TermPart>()) {
                                // [ShapeName]-WithTerm.cshtml
                                // [ShapeName]-[ContentTypeName]-WithTerm.cshtml
                                newShape = shapeName + "__WithTerm";
                                typeShape = shapeName + "__"
                                    + contentItem.ContentType + "__WithTerm";
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(newShape)) {
                            if (!displayedContext.ShapeMetadata.Alternates
                                    .Contains(newShape)) {
                                displayedContext.ShapeMetadata.Alternates
                                    .Add(newShape);
                                displayedContext.ShapeMetadata.Alternates
                                    .Add(typeShape);
                                if (andDisplayType) {
                                    displayedContext.ShapeMetadata.Alternates
                                        .Add(newShape + "__" + displayType);
                                    displayedContext.ShapeMetadata.Alternates
                                        .Add(typeShape + "__" + displayType);
                                }
                            }
                        }
                    }

                });
        }
    }
}