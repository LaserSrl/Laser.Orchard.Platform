using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Filters {
    public class SelectTermsForm : IFormProvider {
        protected dynamic Shape { get; set; }

        public SelectTermsForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "SelectTermsFormCart";

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
                    _FieldIncludeChildren: Shape.FieldSet(
                        Id: "fieldset-include-children",
                        _IncludeChildren: Shape.Checkbox(
                            Id: "IncludeChildren", Name: "IncludeChildren",
                            Title: T("Automatically include children terms in filtering"),
                            Value: "true"
                        )
                    ),
                    _OperatorCart: Shape.SelectList(
                            Id: "operator", Name: "OperatorCart",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        )
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.AllProducts),
                            Text = T("Each product in the cart should have the selected condition").Text
                        })
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.OneProduct),
                            Text = T("At least one product in the cart should have the selected condition").Text
                        })
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.InsideCart),
                            Text = T("Among all the products in the cart, the selected condition must occur").Text
                        })
                    );
                return f;
            };

            context.Form(FormName, form);
        }
    }
}

public enum SelectTermsOperator {
    AllProducts,
    OneProduct,
    InsideCart
}