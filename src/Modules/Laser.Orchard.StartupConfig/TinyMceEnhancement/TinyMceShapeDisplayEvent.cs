using Orchard.DisplayManagement.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    public class TinyMceShapeDisplayEvent : ShapeDisplayEvents {
        public override void Displaying(ShapeDisplayingContext context) {
            if (String.CompareOrdinal(context.ShapeMetadata.Type, "Body_Editor") != 0) {
                return;
            }

            if (!String.Equals(context.Shape.EditorFlavor, "html", StringComparison.InvariantCultureIgnoreCase)) {
                return;
            }

            var aux = context;
        }

    }
}