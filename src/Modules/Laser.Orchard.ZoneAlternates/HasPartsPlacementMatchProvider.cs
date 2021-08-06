using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ZoneAlternates {
    public class HasPartsPlacementMatchProvider : IPlacementParseMatchProvider {
        public string Key {
            get { return "HasParts"; }
        }

        public bool Match(ShapePlacementContext context, string expression) {
            // expression is a comma-separated list of part names
            var partNames = expression
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));
            var match = context.Content != null && context.Content.ContentItem != null;
            if (match) {
                // we will return true if the content has all the parts we selected
                foreach (var name in partNames) {
                    match &= context.Content.ContentItem.Parts
                        .Any(pa => pa.PartDefinition.Name.Equals(name));
                }
            }
            return match;
        }
    }
}