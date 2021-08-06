using Laser.Orchard.NwazetIntegration.Activities;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class WorkflowActtributesValidation : IProductAttributesDriver {
        private readonly IWorkflowManager _workflowManager;
        public WorkflowActtributesValidation(
            IWorkflowManager workflowManager) {

            _workflowManager = workflowManager;
        }
        public dynamic GetAttributeDisplayShape(IContent product, dynamic shapeHelper) {
            return null;
        }

        public bool ValidateAttributes(
            IContent product, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues) {
            // if there is no attribute, we don't need to validate anything
            if (attributeIdsToValues == null || !attributeIdsToValues.Any()) {
                return true;
            }

            var context = new AttributesValidationContext {
                Product = product,
                AttributeIdsToValues = attributeIdsToValues
            };

            _workflowManager.TriggerEvent(
                ValidateAttributesActivity.EventName,
                product,
                () => new Dictionary<string, object> {
                    {"Content", product },
                    {"Context", context }
                });

            return !context.ValidationFailed;
        }
    }
}