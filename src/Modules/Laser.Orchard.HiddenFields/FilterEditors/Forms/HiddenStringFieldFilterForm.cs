using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.HiddenFields.FilterEditors.Forms {
    public class HiddenStringFieldFilterForm : IFormProvider {

        public const string FormName = "HiddenStringFieldFilter";

        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }

        public HiddenStringFieldFilterForm(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// This method creates a form to select the operator to apply.
        /// </summary>
        /// <param name="context"></param>
        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = _shapeFactory.Form(
                        Id: "HiddenStringFieldFilter",
                        _Operator: _shapeFactory.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ),
                        _Value: _shapeFactory.TextBox(
                            Id: "value", Name: "Value",
                            Title: T("Value"),
                            Classes: new[] { "text medium", "tokenized" },
                            Description: T("Enter the value the string should be.")
                            )
                        );

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.NotEquals), Text = T("Is not equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.Contains), Text = T("Contains").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.ContainsAny), Text = T("Contains any word").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.ContainsAll), Text = T("Contains all words").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.Starts), Text = T("Starts with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.NotStarts), Text = T("Does not start with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.Ends), Text = T("Ends with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.NotEnds), Text = T("Does not end with").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.NotContains), Text = T("Does not contain").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.IsEmpty), Text = T("Is empty").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(HiddenStringFieldOperator.IsNotEmpty), Text = T("Is not empty").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

        /// <summary>
        /// This method returns the predicate to be used for the selected operator
        /// </summary>
        /// <param name="formState"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {
            var op = (HiddenStringFieldOperator)Enum.Parse(typeof(HiddenStringFieldOperator), Convert.ToString(formState.Operator));
            object value = Convert.ToString(formState.Value);

            switch (op) {
                case HiddenStringFieldOperator.Equals:
                    return x => x.Eq(property, value);
                case HiddenStringFieldOperator.NotEquals:
                    return x => x.Not(y => y.Eq(property, value));
                case HiddenStringFieldOperator.Contains:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.Anywhere);
                case HiddenStringFieldOperator.ContainsAny:
                    var values1 = Convert.ToString(value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var predicates1 = values1.Skip(1).Select<string, Action<IHqlExpressionFactory>>(x => y => y.Like(property, x, HqlMatchMode.Anywhere)).ToArray();
                    return x => x.Disjunction(y => y.Like(property, values1[0], HqlMatchMode.Anywhere), predicates1);
                case HiddenStringFieldOperator.ContainsAll:
                    var values2 = Convert.ToString(value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var predicates2 = values2.Skip(1).Select<string, Action<IHqlExpressionFactory>>(x => y => y.Like(property, x, HqlMatchMode.Anywhere)).ToArray();
                    return x => x.Conjunction(y => y.Like(property, values2[0], HqlMatchMode.Anywhere), predicates2);
                case HiddenStringFieldOperator.Starts:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.Start);
                case HiddenStringFieldOperator.NotStarts:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.Start));
                case HiddenStringFieldOperator.Ends:
                    return x => x.Like(property, Convert.ToString(value), HqlMatchMode.End);
                case HiddenStringFieldOperator.NotEnds:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.End));
                case HiddenStringFieldOperator.NotContains:
                    return y => y.Not(x => x.Like(property, Convert.ToString(value), HqlMatchMode.Anywhere));
                case HiddenStringFieldOperator.IsEmpty:
                    return x => x.Eq(property, "");
                case HiddenStringFieldOperator.IsNotEmpty:
                    return x => x.Not(y => y.Eq(property, ""));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// This method shows a string describing to the selected filter operator.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="formState"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {
            var op = (HiddenStringFieldOperator)Enum.Parse(typeof(HiddenStringFieldOperator), Convert.ToString(formState.Operator));
            string value = Convert.ToString(formState.Value);

            switch (op) {
                case HiddenStringFieldOperator.Equals:
                    return T("{0} is equal to '{1}'", fieldName, value);
                case HiddenStringFieldOperator.NotEquals:
                    return T("{0} is not equal to '{1}'", fieldName, value);
                case HiddenStringFieldOperator.Contains:
                    return T("{0} contains '{1}'", fieldName, value);
                case HiddenStringFieldOperator.ContainsAny:
                    return T("{0} contains any of '{1}'", fieldName, new LocalizedString(String.Join("', '", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))));
                case HiddenStringFieldOperator.ContainsAll:
                    return T("{0} contains all '{1}'", fieldName, new LocalizedString(String.Join("', '", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))));
                case HiddenStringFieldOperator.Starts:
                    return T("{0} starts with '{1}'", fieldName, value);
                case HiddenStringFieldOperator.NotStarts:
                    return T("{0} does not start with '{1}'", fieldName, value);
                case HiddenStringFieldOperator.Ends:
                    return T("{0} ends with '{1}'", fieldName, value);
                case HiddenStringFieldOperator.NotEnds:
                    return T("{0} does not end with '{1}'", fieldName, value);
                case HiddenStringFieldOperator.NotContains:
                    return T("{0} does not contain '{1}'", fieldName, value);
                case HiddenStringFieldOperator.IsEmpty:
                    return T("{0} is empty", fieldName);
                case HiddenStringFieldOperator.IsNotEmpty:
                    return T("{0} is not empty", fieldName);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}