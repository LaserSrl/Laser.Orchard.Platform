using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Alternates {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ContentShortCodeAlternates : ShapeDisplayEvents {
        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
            .OnDisplaying((displayedContext) => {
                if (displayedContext.Shape.ShortCoded != null && displayedContext.Shape.ShortCoded is bool && displayedContext.Shape.ShortCoded) {
                    //Adds alternates for Children
                    foreach (var child in displayedContext.Shape.Content.Items) {
                        var childAlternates = new List<string>(new[] { (string)child.Metadata.Type + "__Shortcoded" });
                        foreach (var alternate in ((IList<string>)child.Metadata.Alternates).Distinct()) {
                            if (alternate.IndexOf("__Shortcoded") < 0) {
                                childAlternates.Add(alternate + "__Shortcoded");
                            }
                        }
                        if (childAlternates.Count() > 0) {
                            child.Metadata.Alternates.AddRange(childAlternates);
                        }
                    }
                    var alternates = new List<string>(new[] { (string)displayedContext.Shape.Metadata.Type + "__Shortcoded" });
                    foreach (var alternate in ((IList<string>)displayedContext.Shape.Metadata.Alternates).Distinct()) {
                        if (alternate.IndexOf("__Shortcoded") < 0) {
                            alternates.Add(alternate + "__Shortcoded");
                        }
                    }
                    if (alternates.Count() > 0) {
                        displayedContext.Shape.Metadata.Alternates.AddRange(alternates);
                    }
                }
            });
        }

    }
}