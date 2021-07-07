using Contrib.Voting.Models;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FilterEditors.Forms;
using OProjections = Orchard.Projections.Services;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Contrib.Voting.Filters {
    public class VoteFilterBase : OProjections.IFilterProvider {
        protected IRepository<ResultRecord> _resultRecords;
        protected string functionName = string.Empty;

        public Localizer T { get; set; }
                
        public virtual void Describe(DescribeFilterContext describe) {
           
        }

        public void ApplyFilter(FilterContext context) {
            List<int> contentIds = new List<int>();
            decimal dvalue, dmin, dmax;
            dvalue = FromStateValue(context.State.Value);
            dmin = FromStateValue(context.State.Min);
            dmax = FromStateValue(context.State.Max);
            //bool showNotReviewed = false;
            var op = (NumericOperator)Enum.Parse(
                typeof(NumericOperator), Convert.ToString(context.State.Operator));

            // LEAVING COMMENTED CODE FOR FUTURE INSPIRATION / USE.
            //if (context.State.ShowAllIfNoValue.Value == "on") {
            //    showNotReviewed = true;
            //}

            // If no valid value is in my context, I don't have to apply this filter.
            switch (op) {
                case NumericOperator.Between:
                case NumericOperator.NotBetween:
                    if (dmin == 0 && dmax == 0) return;
                    break;

                case NumericOperator.Equals:
                case NumericOperator.NotEquals:
                    if (dvalue == 0) return;
                    break;

                case NumericOperator.GreaterThan:
                case NumericOperator.GreaterThanEquals:
                    if (dmin == 0) return;
                    break;

                case NumericOperator.LessThan:
                case NumericOperator.LessThanEquals:
                    if (dmax == 0) return;
                    break;

                default:
                    break;
            }

            contentIds = _resultRecords
                .Fetch(
                    GetFilterByOperator(op, dvalue, dmin, dmax/*, showNotReviewed*/)
                    )
                .Select(y => y.ContentItemRecord.Id)
                .Distinct().ToList();

            // LEAVING COMMENTED CODE FOR FUTURE INSPIRATION / USE.
            //// To show records having no reviews, I need to invert my filter.
            //if (showNotReviewed) {
            //    context.Query.Where(x => x.ContentItem(), y => y.Not(z => z.In("Id", contentIds.ToArray())));
            //} else {
            context.Query.Where(x => x.ContentItem(), y => y.In("Id", contentIds.ToArray()));
            //}
        }

        public virtual LocalizedString DisplayFilter(FilterContext context) {
            return T("Votes / reviews filter");
        }

        protected Expression<Func<ResultRecord, bool>> GetFilterByOperator(NumericOperator op, decimal val, decimal min, decimal max/*, bool showNotReviewed*/) {
            // LEAVING COMMENTED CODE FOR FUTURE INSPIRATION / USE.
            //// My starting _resultRecords list contains elements which have at least one review.
            //// To show records having no reviews, I need to invert my filter.
            //// E.g. the Between operator needs to work like the NotBetween operator.
            switch (op) {
                case NumericOperator.Between:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && ((decimal)x.Value < min || (decimal)x.Value > max);
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value >= min && (decimal)x.Value <= max;

                case NumericOperator.Equals:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value != val;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value == val;

                case NumericOperator.GreaterThan:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value <= min;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value > min;

                case NumericOperator.GreaterThanEquals:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value < min;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value >= min;

                case NumericOperator.LessThan:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value >= max;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value < max;

                case NumericOperator.LessThanEquals:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value > max;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value <= max;

                case NumericOperator.NotBetween:
                    //if (showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value >= min && (decimal)x.Value <= max;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && ((decimal)x.Value < min || (decimal)x.Value > max);

                case NumericOperator.NotEquals:
                    //if(showNotReviewed) {
                    //    return x => x.FunctionName.ToUpper() == "AVERAGE" && (decimal)x.Value == val;
                    //}
                    return x => x.FunctionName.ToUpper() == functionName.ToUpper() && (decimal)x.Value != val;

                default:
                    return null;
            }
        }

        protected decimal FromStateValue(dynamic sValue) {
            var str = Convert.ToString(sValue);
            decimal value;
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                return value;
            }
            return 0m;
        }
    }

    public class VoteFilterForm : IFormProvider {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }
        protected Work<IResourceManager> _resourceManager;

        protected string FormName = string.Empty;

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: FormName,
                       _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ),
                       _FieldSetSingle: Shape.FieldSet(
                           Id: "fieldset-single",
                           _Value: Shape.TextBox(
                               Id: "value", Name: "Value",
                               Title: T("Value"),
                               Classes: new[] { "tokenized" }
                               )
                           ),
                       _FieldSetMin: Shape.FieldSet(
                           Id: "fieldset-min",
                           _Min: Shape.TextBox(
                               Id: "min", Name: "Min",
                               Title: T("Min"),
                               Classes: new[] { "tokenized" }
                               )
                           ),
                       _FieldSetMax: Shape.FieldSet(
                           Id: "fieldset-max",
                           _Max: Shape.TextBox(
                               Id: "max", Name: "Max",
                               Title: T("Max"),
                               Classes: new[] { "tokenized" }
                               )
                           )/*, // LEAVING COMMENTED CODE FOR FUTURE INSPIRATION / USE.
                       _FieldSetShowNotReviewed: Shape.FieldSet(
                           Id: "fieldset-not-reviewed",
                           _ShowNotReviewed: Shape.Checkbox(
                               Id: "notReviewed", Name: "NotReviewed",
                               Title: T("Show not reviewed"),
                               Classes: new[] { "tokenized" }
                               )
                           )*/
                   );
                    _resourceManager.Value.Require("script", "jQuery");
                    _resourceManager.Value.Include("script", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js");

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThan), Text = T("Is less than").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThanEquals), Text = T("Is less than or equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.NotEquals), Text = T("Is not equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThanEquals), Text = T("Is greater than or equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThan), Text = T("Is greater than").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Between), Text = T("Is between").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.NotBetween), Text = T("Is not between").Text });
                    return f;
                };
            context.Form(FormName, form);
        }
    }
}