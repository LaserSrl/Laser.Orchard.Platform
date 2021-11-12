using Laser.Orchard.StartupConfig.ShortCodes.Abstractions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ShortCodes.Providers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class EmptyShortCodeProvider : IShortCodeProvider {
        public Descriptor Describe() {
            return new Descriptor {
                Signature = "emptyshortcodeprovider"
            };
        }

        public void Evaluate(EvaluateContext context) {
            
        }
    }
}