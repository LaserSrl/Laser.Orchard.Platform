using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class HeaderValueForm : IFormProvider {
        public const string FormName = "HeaderValueForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public HeaderValueForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = Shape.Form(
                        Id: FormName,
                        _Key: Shape.TextBox(
                            Id: "name", Name: "Name",
                            Title: T("Name"),
                            Classes: new[] { "text medium", "tokenized", "required" },
                            Required: true,
                            Description: T("Enter the name of the header.")),
                        _Value: Shape.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: T("Enter the value for the header.")),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false)
                        );

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(HeaderValueOperator.RequestHasHeaderWithAnyValue),
                        Text = T("Request has Header (with any value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(HeaderValueOperator.RequestHasHeaderWithSpecificValue),
                        Text = T("Request has Header (with specific value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(HeaderValueOperator.RequestHasHeaderWithDifferentValue),
                        Text = T("Request has Header (with different value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(HeaderValueOperator.RequestDoesntHaveHeader),
                        Text = T("Request doesn't have Header").Text
                    });

                    return f;
                };

            context.Form(FormName, form);
        }

        public static LocalizedString DisplayFilter(dynamic formState, Localizer T) {
            var op = (HeaderValueOperator)Enum.Parse(typeof(HeaderValueOperator), Convert.ToString(formState.Operator));

            var name = Convert.ToString(formState.Name);
            var value = Convert.ToString(formState.Value);

            switch (op) {
                case HeaderValueOperator.RequestHasHeaderWithAnyValue:
                    return T("Request has header '{0}' with any value", name);
                case HeaderValueOperator.RequestHasHeaderWithSpecificValue:
                    return T("Request has header '{0}' with value '{1}'", name, value);
                case HeaderValueOperator.RequestHasHeaderWithDifferentValue:
                    return T("Request has header '{0}' with value different than '{1}'", name, value);
                case HeaderValueOperator.RequestDoesntHaveHeader:
                    return T("Request doesn't have header '{0}'", name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Func<HttpRequestBase, bool> GetFilterPredicate(dynamic formState) {
            var op = (HeaderValueOperator)Enum.Parse(typeof(HeaderValueOperator), Convert.ToString(formState.Operator));

            var name = Convert.ToString(formState.Name);
            var value = Convert.ToString(formState.Value);

            switch (op) {
                case HeaderValueOperator.RequestHasHeaderWithAnyValue:
                    return req => req.Headers[name] != null;
                case HeaderValueOperator.RequestHasHeaderWithSpecificValue:
                    return req => {
                        var headerValue = (string)req.Headers[name];
                        return headerValue != null 
                            && headerValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
                    };
                case HeaderValueOperator.RequestHasHeaderWithDifferentValue:
                    return req => {
                        var headerValue = (string)req.Headers[name];
                        return headerValue != null
                            && !headerValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
                    };
                case HeaderValueOperator.RequestDoesntHaveHeader:
                    return req => req.Headers[name] == null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class HeaderValueFormValidator : IFormEventHandler {

        public Localizer T { get; set; }

        public HeaderValueFormValidator() {
            T = NullLocalizer.Instance;
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != HeaderValueForm.FormName) {
                return;
            }

            // name of header to test is required
            if (string.IsNullOrWhiteSpace(context
                .ValueProvider.GetValue("Name").AttemptedValue)) {
                context.ModelState
                    .AddModelError("Name",
                        T("Insert a value for the Name of the header to test (it is allowed to be a token).").Text);
            }
        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }

    public enum HeaderValueOperator {
        RequestHasHeaderWithAnyValue,
        RequestHasHeaderWithSpecificValue,
        RequestHasHeaderWithDifferentValue,
        RequestDoesntHaveHeader
    }
}