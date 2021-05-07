using Laser.Orchard.SecureData.Fields;
using Laser.Orchard.SecureData.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Filters {
    public class EncryptedStringFieldFilterProvider : IFilterProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IEnumerable<IFieldTypeEditor> _fieldTypeEditors;
        private readonly ISecureFieldService _secureFieldService;

        public Localizer T { get; set; }

        public EncryptedStringFieldFilterProvider(IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentFieldDriver> contentFieldDrivers,
                IEnumerable<IFieldTypeEditor> fieldTypeEditors,
                ISecureFieldService secureFieldService) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            _fieldTypeEditors = fieldTypeEditors;
            _secureFieldService = secureFieldService;

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe) {
            foreach (var part in _contentDefinitionManager.ListPartDefinitions()) {
                // This query returns all EncryptedStringFieldDefinitions in the parts where there's at least one EncryptedStringField.
                var encryptedStringFieldDefinitions = part.Fields.Where(fd => fd.FieldDefinition.Name.Equals("EncryptedStringField"));
                var localPart = part;

                foreach (var field in encryptedStringFieldDefinitions) {
                    var localField = field;

                    // The following code generates the list of filters you can apply on EncryptedStringFields.
                    // Fields are grouped by part.
                    describe
                        .For(
                            part.Name + "ContentFields",
                            T("{0} Content Fields", part.Name.CamelFriendly()),
                            T("Content Fields for {0}", part.Name.CamelFriendly()))
                        .Element(
                            localPart.Name + "." + localField.Name,
                            T(localField.DisplayName),
                            T("Value for {0}", localField.DisplayName),
                            context => ApplyFilter(context, part, field),
                            DisplayFilter,
                            EncryptedStringFieldForm.FormName);
                }
            }
        }

        public void ApplyFilter(FilterContext context, ContentPartDefinition part, ContentPartFieldDefinition field) {
            string rawValue = Convert.ToString(context.State.Value);
            bool returnAllElements = false;

            string value = string.Empty;

            // CheckBox True Value in Form is "on".
            if (context.State.ShowAllIfNoValue.Value == "on" && string.IsNullOrWhiteSpace(rawValue)) {
                returnAllElements = true;
            } else {
                value = _secureFieldService.EncodeString(rawValue, part.Name + "." + field.Name);
            }

            string propertyName = String.Join(".", part.Name, field.Name, "");

            // This is the actual join between the FieldIndexPartRecord table and the StringFieldIndexRecords table, which contains the values of each property.
            Action<IAliasFactory> relationship = x => x.ContentPartRecord<FieldIndexPartRecord>().Property("StringFieldIndexRecords", propertyName.ToSafeName());

            // Search on the "PropertyName" column the name of the property to apply the filter to.
            Action<IHqlExpressionFactory> propertyFilter = x => x.Eq("PropertyName", propertyName);

            var query = (IHqlQuery)context.Query;

            if (!returnAllElements) {
                // Add the filter on the "Value" column.
                Action<IHqlExpressionFactory> completeFilter = x => x.And(y => y.Eq("Value", value), propertyFilter);

                context.Query = query
                    .Where(relationship, completeFilter);

                return;
            }

            // This is the actual query execution, applying the filters ("completeFilter" variable) on the table / view created with "relationships" variable.
            context.Query = query
                    .Where(relationship, propertyFilter);

            return;
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string description = "Value of the field is equal to '{0}'.";

            if (context.State.ShowAllIfNoValue.Value == "on") {
                description += " If '{0}' is empty, returns all results.";
            }

            return T(description, Convert.ToString(context.State.Value));
        }
    }
}