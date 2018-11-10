using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyExtensionPart : ContentPart {
        public OrderType OrderBy {
            get { return this.Retrieve(r => r.OrderBy); }
            set { this.Store(r => r.OrderBy, value); }
        }
    }
}