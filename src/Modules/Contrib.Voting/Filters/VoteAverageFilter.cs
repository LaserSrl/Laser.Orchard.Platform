using Contrib.Voting.Models;
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
                    "AverageVote",
                    T("Average Vote"),
                    T("Average vote for the contents"),
                    ApplyFilter,
                    DisplayFilter,
                    VoteAverageFilterForm.FormName
                );
        }

        public override LocalizedString DisplayFilter(FilterContext context) {
            return T("Average rating");
        }
    }
}