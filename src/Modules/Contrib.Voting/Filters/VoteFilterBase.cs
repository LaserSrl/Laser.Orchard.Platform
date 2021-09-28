using Contrib.Voting.Models;
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
using System.Web.Mvc;

namespace Contrib.Voting.Filters {
    public abstract class VoteFilterBase : OProjections.IFilterProvider {
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
            var op = (NumericOperator)Enum.Parse(
                typeof(NumericOperator), Convert.ToString(context.State.Operator));
            bool showNotReviewed = false;
            if (context.State.NotReviewed != null && context.State.NotReviewed.Value == "true") {
                showNotReviewed = true;
            }

            // If no valid value is in my context, I don't have to apply this filter.
            switch (op) {
                case NumericOperator.Between:
                case NumericOperator.NotBetween:
                    if (dmin == 0 && dmax == 0) return;
                    break;

                case NumericOperator.Equals:
                case NumericOperator.NotEquals:
                case NumericOperator.GreaterThan:
                case NumericOperator.GreaterThanEquals:
                case NumericOperator.LessThan:
                case NumericOperator.LessThanEquals:
                    if (dvalue == 0) return;
                    break;

                default:
                    return;
            }
            
            string sqlStrBase = "SELECT rr.ContentItemRecord.Id FROM Contrib.Voting.Models.ResultRecord AS rr";

            string sqlStr;
            Dictionary<string, object> sqlParams;
            GetHQLStatement(op, dvalue, dmin, dmax, out sqlStr, out sqlParams);

            // If I need to show elements that have not been reviewed, the query has to be something like:
            // ... WHERE Id IN (filtered query on voting table) OR Id NOT IN (unfiltered query on voting table)
            if (showNotReviewed) {
                context.Query.Where(x => x.ContentItem(), a => a
                    .Or(b => b
                        .Not(c => c
                            .InSubquery("Id", sqlStrBase, new Dictionary<string, object>())),
                    y => y.InSubquery("Id", sqlStr, sqlParams)));
            } else {
                context.Query.Where(x => x.ContentItem(), y => y.InSubquery("Id", sqlStr, sqlParams));
            }
        }

        private void GetHQLStatement(NumericOperator op, decimal val, decimal min, decimal max,
                                    out string sqlStr, out Dictionary<string,object> sqlParams) {
            sqlParams = new Dictionary<string, object>();
            sqlStr = "SELECT rr.ContentItemRecord.Id FROM Contrib.Voting.Models.ResultRecord AS rr WHERE rr.FunctionName = :functionname AND ";
            sqlParams.Add("functionname", functionName);

            switch (op) {
                case NumericOperator.Between:
                    sqlStr += "rr.Value BETWEEN :min AND :max";
                    sqlParams.Add("min", min);
                    sqlParams.Add("max", max);
                    break;

                case NumericOperator.Equals:
                    sqlStr += "rr.Value = :value";
                    sqlParams.Add("value", val);
                    break;

                case NumericOperator.GreaterThan:
                    sqlStr += "rr.Value > :value";
                    sqlParams.Add("value", val);
                    break;

                case NumericOperator.GreaterThanEquals:
                    sqlStr += "rr.Value >= :value";
                    sqlParams.Add("value", val);
                    break;

                case NumericOperator.LessThan:
                    sqlStr += "rr.Value < :value";
                    sqlParams.Add("value", val);
                    break;

                case NumericOperator.LessThanEquals:
                    sqlStr += "rr.Value <= :value";
                    sqlParams.Add("value", val);
                    break;

                case NumericOperator.NotBetween:
                    sqlStr += "rr.Value NOT BETWEEN :min AND :max";
                    sqlParams.Add("min", min);
                    sqlParams.Add("max", max);
                    break;

                case NumericOperator.NotEquals:
                    sqlStr += "rr.Value <> :value";
                    sqlParams.Add("value", val);
                    break;
            }
        }

        public virtual LocalizedString DisplayFilter(FilterContext context) {
            return T("Votes / reviews filter");
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

    public abstract class VoteFilterForm : IFormProvider {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }
        protected Work<IResourceManager> _resourceManager;

        protected string _formName { get; set; }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: _formName,
                       _Operator: Shape.SelectList(
                            Id: "operator", 
                            Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        ),
                       _FieldSetSingle: Shape.FieldSet(
                           Id: "fieldset-single",
                           _Value: Shape.TextBox(
                               Id: "value", 
                               Name: "Value",
                               Title: T("Value"),
                               Classes: new[] { "tokenized" }
                               )
                           ),
                       _FieldSetMin: Shape.FieldSet(
                           Id: "fieldset-min",
                           _Min: Shape.TextBox(
                               Id: "min", 
                               Name: "Min",
                               Title: T("Min"),
                               Classes: new[] { "tokenized" }
                               )
                           ),
                       _FieldSetMax: Shape.FieldSet(
                           Id: "fieldset-max",
                           _Max: Shape.TextBox(
                               Id: "max", 
                               Name: "Max",
                               Title: T("Max"),
                               Classes: new[] { "tokenized" }
                               )
                           ),
                       _FieldSetShowNotReviewed: Shape.FieldSet(
                           Id: "fieldset-not-reviewed",
                           _ShowNotReviewed: Shape.Checkbox(
                               Id: "notReviewed", 
                               Name: "NotReviewed",
                               Title: T("Show not reviewed"),
                               Value: "true"
                               )
                           )
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
            context.Form(_formName, form);
        }
    }
}