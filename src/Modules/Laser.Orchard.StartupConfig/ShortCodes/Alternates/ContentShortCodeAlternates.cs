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
            .OnDisplaying(displayedContext => {
                if (displayedContext.Shape.ShortCoded)
                    displayedContext.ShapeMetadata.Alternates.Add("ShortCoded");
            });
        }
    }
}