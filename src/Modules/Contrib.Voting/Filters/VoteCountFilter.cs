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
                    "VoteCount",
                    T("Vote count"),
                    T("Number of votes for the contents"),
                    ApplyFilter,
                    DisplayFilter,
                    VoteCountFilterForm.FormName
                );
        }
        
        public override LocalizedString DisplayFilter(FilterContext context) {
            return T("Number of votes / reviews");
        }
    }
}