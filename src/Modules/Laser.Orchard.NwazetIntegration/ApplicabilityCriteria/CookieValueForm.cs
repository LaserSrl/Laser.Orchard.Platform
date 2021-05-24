using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CookieValueForm : IFormProvider {
        public const string FormName = "CookieValueForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public CookieValueForm(IShapeFactory shapeFactory) {
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
                            Description: T("Enter the name of the cookie.")),
                        _Value: Shape.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: T("Enter the value for the cookie.")),
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false)
                        );

                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CookieValueOperator.RequestHasCookieWithAnyValue),
                        Text = T("Request has Cookie (with any value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CookieValueOperator.RequestHasCookieWithSpecificValue),
                        Text = T("Request has Cookie (with specific value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CookieValueOperator.RequestHasCookieWithDifferentValue),
                        Text = T("Request has Cookie (with different value)").Text
                    });
                    f._Operator.Add(new SelectListItem {
                        Value = Convert.ToString(CookieValueOperator.RequestDoesntHaveCookie),
                        Text = T("Request doesn't have Cookie").Text
                    });

                    return f;
                };

            context.Form(FormName, form);
        }

        public static LocalizedString DisplayFilter(dynamic formState, Localizer T) {
            var op = (CookieValueOperator)Enum.Parse(typeof(CookieValueOperator), Convert.ToString(formState.Operator));

            var name = Convert.ToString(formState.Name);
            var value = Convert.ToString(formState.Value);

            switch (op) {
                case CookieValueOperator.RequestHasCookieWithAnyValue:
                    return T("Request has cookie '{0}' with any value", name);
                case CookieValueOperator.RequestHasCookieWithSpecificValue:
                    return T("Request has cookie '{0}' with value '{1}'", name, value);
                case CookieValueOperator.RequestHasCookieWithDifferentValue:
                    return T("Request has cookie '{0}' with value different than '{1}'", name, value);
                case CookieValueOperator.RequestDoesntHaveCookie:
                    return T("Request doesn't have cookie '{0}'", name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Func<HttpRequestBase, bool> GetFilterPredicate(dynamic formState) {
            var op = (CookieValueOperator)Enum.Parse(typeof(CookieValueOperator), Convert.ToString(formState.Operator));

            var name = Convert.ToString(formState.Name);
            var value = Convert.ToString(formState.Value);

            switch (op) {
                case CookieValueOperator.RequestHasCookieWithAnyValue:
                    return req => req.Cookies[name] != null;
                case CookieValueOperator.RequestHasCookieWithSpecificValue:
                    return req => {
                        var cookieValue = (string)(req.Cookies[name]?.Value);
                        return cookieValue != null 
                            && cookieValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
                    };
                case CookieValueOperator.RequestHasCookieWithDifferentValue:
                    return req => {
                        var cookieValue = (string)(req.Cookies[name]?.Value);
                        return cookieValue != null
                            && !cookieValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
                    };
                case CookieValueOperator.RequestDoesntHaveCookie:
                    return req => req.Cookies[name] == null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CookieValueFormValidator : IFormEventHandler {

        public Localizer T { get; set; }

        public CookieValueFormValidator() {
            T = NullLocalizer.Instance;
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName != CookieValueForm.FormName) {
                return;
            }

            // name of cookie to test is required
            if (string.IsNullOrWhiteSpace(context
                .ValueProvider.GetValue("Name").AttemptedValue)) {
                context.ModelState
                    .AddModelError("Name",
                        T("Insert a value for the Name of the cookie to test (it is allowed to be a token).").Text);
            }
        }

        #region Methods not implemented
        public void Building(BuildingContext context) { }

        public void Built(BuildingContext context) { }

        public void Validated(ValidatingContext context) { }
        #endregion
    }

    public enum CookieValueOperator {
        RequestHasCookieWithAnyValue,
        RequestHasCookieWithSpecificValue,
        RequestHasCookieWithDifferentValue,
        RequestDoesntHaveCookie
    }
}