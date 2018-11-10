using System;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Projections {
    public class OwnerStringForm : IFormProvider {
        public const string FormName = "OwnerStringForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public OwnerStringForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "OwnerStringForm",
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ),
                        _Value: Shape.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: T("Enter the value the string should be, if more than one divided by a comma.")
                            )
                        );

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotEquals), Text = T("Is not equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Contains), Text = T("Contains").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.ContainsAny), Text = T("Contains any word").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Starts), Text = T("Starts with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotStarts), Text = T("Does not start with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.Ends), Text = T("Ends with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotEnds), Text = T("Does not end with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(StringOperator.NotContains), Text = T("Does not contain").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

    }

    public enum StringOperator {
        Equals,
        NotEquals,
        Contains,
        ContainsAny,
        ContainsAll,
        Starts,
        NotStarts,
        Ends,
        NotEnds,
        NotContains,
    }
}