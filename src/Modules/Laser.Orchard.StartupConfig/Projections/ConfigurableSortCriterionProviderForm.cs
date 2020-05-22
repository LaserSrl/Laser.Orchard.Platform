using Newtonsoft.Json;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Projections {
    public class ConfigurableSortCriterionProviderForm : IFormProvider {

        public const string FormName = "ConfigurableSortCriterionProviderForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ConfigurableSortCriterionProviderForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "ConfigurableSortCriterionProviderForm",
                    _CriterionIndex: Shape.TextBox(
                        Id: "criterion_index",
                        Name: "CriterionIndex",
                        Title: T("Index of the sort criterion."),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("Enter the index of the criterion to be used. This will be interpreted as an integer index into the array given here in JSON. If this is not an integer, or if it's out of the bounds of the array, the first sort criterion will be used.")),
                    _CriteriaArray: Shape.TextArea(
                        Id: "criteria_array",
                        Name: "CriteriaArray",
                        Title: T("Array of objects describing the sort criteria."),
                        Classes: new[] { "tokenized" },
                        Description: T("TODO"))
                    );

                return f;
            };

            context.Form(FormName, form);
        }
    }

    public class ConfigurableSortCriterionProviderFormValidator : IFormEventHandler {

        public Localizer T { get; set; }

        public ConfigurableSortCriterionProviderFormValidator() {
            T = NullLocalizer.Instance;
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != ConfigurableSortCriterionProviderForm.FormName) {
                return;
            }

            if (string.IsNullOrWhiteSpace(context
                .ValueProvider.GetValue("CriterionIndex").AttemptedValue)) {
                context.ModelState
                    .AddModelError("CriterionIndex", 
                        T("Insert a value for the Index (it is allowed to be a token).").Text);
            }

            try {
                var json = context
                    .ValueProvider.GetValue("CriteriaArray").AttemptedValue;
                var criteria = JsonConvert
                    .DeserializeObject<SortCriterionConfiguration[]>(json);
                if (criteria == null || !criteria.Any()) {
                    context.ModelState
                        .AddModelError("CriteriaArray",
                            T("The array of Criteria may not be null").Text);
                }
            } catch (Exception ex) {
                context.ModelState
                    .AddModelError("CriteriaArray",
                        T("Error while parsing the json array: " + ex.Message).Text);
            }
        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }
}