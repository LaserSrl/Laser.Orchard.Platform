using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections {
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class SelectTermsForm : IFormProvider {
        protected dynamic Shape { get; set; }

        public SelectTermsForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "SelectTermsForm";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "SelectTermsForm",
                    _Terms: Shape.TextBox(
                        Id: "terms",
                        Name: "Terms",
                        Title: T("Terms"),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("The term identifiers. If no valid term is provided, all content items will be included in the results.")),
                    _Exclusion: Shape.FieldSet(
                        _OperatorOneOf: Shape.Radio(
                            Id: "operator-is-one-of", Name: "Operator",
                            Title: T("Is associated to one of the specified terms"), Value: "0", Checked: true
                            ),
                        _OperatorIsAllOf: Shape.Radio(
                            Id: "operator-is-all-of", Name: "Operator",
                            Title: T("Is associated to all the specified terms"), Value: "1"
                            )
                        ),
                    _IncludeChildren: Shape.Checkbox(
                        Id: "IncludeChildren", Name: "IncludeChildren",
                        Title: T("Automatically include children terms in filtering"),
                        Value: "true"
                    ));
                return f;
            };

            context.Form(FormName, form);
        }
    }
}