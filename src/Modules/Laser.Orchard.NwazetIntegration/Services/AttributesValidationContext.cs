using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class AttributesValidationContext {
        // Inputs
        public IContent Product { get; set; }
        public IDictionary<int, ProductAttributeValueExtended> AttributeIdsToValues { get; set; }
        // Outputs
        public bool ValidationFailed { get; set; }
    }
}