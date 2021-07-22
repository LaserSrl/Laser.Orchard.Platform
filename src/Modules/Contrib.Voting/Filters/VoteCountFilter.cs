﻿using Contrib.Voting.Models;
using Nwazet.Commerce.Filters;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Contrib.Voting.Filters {
    public class VoteCountFilter : VoteFilterBase {
        public VoteCountFilter(IRepository<ResultRecord> resultRecords) {
            _resultRecords = resultRecords;
            T = NullLocalizer.Instance;
            functionName = "COUNT";
        }

        public override void Describe(DescribeFilterContext describe) {
            describe
                .For(
                    "Reviews",
                    T("Reviews"),
                    T("Reviews"))
                .Element(
                    "ReviewCount",
                    T("Review count"),
                    T("Number of reviews for the contents."),
                    ApplyFilter,
                    DisplayFilter,
                    VoteCountFilterForm.FormName
                );
        }

        public override LocalizedString DisplayFilter(FilterContext context) {
            string value = context.State.Value;
            string min = context.State.Min;
            string max = context.State.Max;
            var op = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(context.State.Operator));

            bool showNotReviewed = false;

            if (context.State.NotReviewed != null && context.State.NotReviewed.Value == "true") {
                showNotReviewed = true;
            }

            switch (op) {
                case NumericOperator.Between:
                    if (showNotReviewed) {
                        return T("Review count rating is between {0} and {1}", min, max);
                    }
                    return T("Review count is between {0} and {1}. Items with no votes will not be displayed.", min, max);

                case NumericOperator.Equals:
                    if (showNotReviewed) {
                        return T("Review count is equals to {0}", value);
                    }
                    return T("Review count is equals to {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.GreaterThan:
                    if (showNotReviewed) {
                        return T("Review count is greater than {0}", value);
                    }
                    return T("Review count is greater than {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.GreaterThanEquals:
                    if (showNotReviewed) {
                        return T("Review count is greater than or equal to {0}", value);
                    }
                    return T("Review count is greater than or equal to {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.LessThan:
                    if (showNotReviewed) {
                        return T("Review count is less than {0}", value);
                    }
                    return T("Review count is less than {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.LessThanEquals:
                    if (showNotReviewed) {
                        return T("Review count is less than or equal to {0}", value);
                    }
                    return T("Review count is less than or equal to {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.NotBetween:
                    if (showNotReviewed) {
                        return T("Review count is not between {0} and {1}", min, max);
                    }
                    return T("Review count is not between {0} and {1}. Items with no votes will not be displayed.", min, max);

                case NumericOperator.NotEquals:
                    if (showNotReviewed) {
                        return T("Review count is not equal to {0}", value);
                    }
                    return T("Review count is not equal to {0}. Items with no votes will not be displayed.", value);
            }

            if (showNotReviewed) {
                return T("Review count");
            }
            return T("Review count. Items with no votes will not be displayed.");
        }
    }
}