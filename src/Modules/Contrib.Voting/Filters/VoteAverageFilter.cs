using Contrib.Voting.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FilterEditors.Forms;
using System;

namespace Contrib.Voting.Filters {
    public class VoteAverageFilter : VoteFilterBase {
        public VoteAverageFilter(IRepository<ResultRecord> resultRecords) {
            _resultRecords = resultRecords;
            T = NullLocalizer.Instance;
            functionName = "AVERAGE";
        }

        public override void Describe(DescribeFilterContext describe) {
            describe
                .For(
                    "Reviews",
                    T("Reviews"),
                    T("Reviews"))
                .Element(
                    "AverageRating",
                    T("Average Rating"),
                    T("Average rating for the contents."),
                    ApplyFilter,
                    DisplayFilter,
                    VoteAverageFilterForm.FormName
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
                        return T("Average rating is between {0} and {1}", min, max);
                    }
                    return T("Average rating is between {0} and {1}. Items with no votes will not be displayed.", min, max);

                case NumericOperator.Equals:
                    if (showNotReviewed) {
                        return T("Average rating is equals to {0}", value);
                    }
                    return T("Average rating is equals to {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.GreaterThan:
                    if (showNotReviewed) {
                        return T("Average rating is greater than {0}", value);
                    }
                    return T("Average rating is greater than {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.GreaterThanEquals:
                    if (showNotReviewed) {
                        return T("Average rating is greater than or equal to {0}", value);
                    }
                    return T("Average rating is greater than or equal to {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.LessThan:
                    if (showNotReviewed) {
                        return T("Average rating is less than {0}", value);
                    }
                    return T("Average rating is less than {0}. Items with no votes will not be displayed.", value);

                case NumericOperator.LessThanEquals:
                    if (showNotReviewed) {
                        return T("Average rating is less than or equal to {0}", value);
                    }
                    return T("Average rating is less than or equal to {0}. Items with no votes will not be displayed.", value);
                  
                case NumericOperator.NotBetween:
                    if (showNotReviewed) {
                        return T("Average rating is not between {0} and {1}", min, max);
                    }
                    return T("Average rating is not between {0} and {1}. Items with no votes will not be displayed.", min, max);

                case NumericOperator.NotEquals:
                    if (showNotReviewed) {
                        return T("Average rating is not equal to {0}", value);
                    }
                    return T("Average rating is not equal to {0}. Items with no votes will not be displayed.", value);
            }

            if (showNotReviewed) {
                return T("Average rating");
            }
            return T("Average rating. Items with no votes will not be displayed.");
        }
    }
}