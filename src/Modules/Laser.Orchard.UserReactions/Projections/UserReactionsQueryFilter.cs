using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Razor.Tokenizer;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQueryFilter : IFilterProvider {
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        public Localizer T { get; set; }
        //private readonly ITokenizer _tokenizer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repoSummary"></param>
        public UserReactionsQueryFilter(IRepository<UserReactionsSummaryRecord> repoSummary){//, ITokenizer tokenizer 
            _repoSummary = repoSummary;
            T = NullLocalizer.Instance;
            //_tokenizer = tokenizer;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="describe"></param>
        public void Describe(global::Orchard.Projections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Search", T("Search reactions"), T("Search reactions"))
                .Element("ReactionsFilter", T("Reactions filter"), T("Filter for user reactions."),
                    ApplyFilter,
                    DisplayFilter,
                    "ReactionsFilterForm"
                );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ApplyFilter(FilterContext context) {
            string reaction = context.State.Reaction;
            var op = (UserReactionsFieldOperator)Enum.Parse(typeof(UserReactionsFieldOperator), Convert.ToString(context.State.Operator));
            int value = ((context.State.Value!=string.Empty) ? Convert.ToInt32(context.State.Value):0);
            int min = ((context.State.Min!="")?Convert.ToInt32(context.State.Min):0);
            int max = ((context.State.Max != "") ? Convert.ToInt32(context.State.Max) : 0);

            context.Query.Join(a => a.ContentPartRecord<UserReactionsPartRecord>()
                .Property("Reactions", "reactionsSummary")
                .Property("UserReactionsTypesRecord", "reactionType"));
            context.Query.Where(a => a.Named("reactionType"), x => x.Eq("TypeName", reaction));

            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Lt("Quantity", value));
                    break;
                case UserReactionsFieldOperator.LessThanEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Le("Quantity", value));
                    break;
                case UserReactionsFieldOperator.Equals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Eq("Quantity", value));
                    break;
                case UserReactionsFieldOperator.NotEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Not(z => z.Eq("Quantity", value)));
                    break;
                case UserReactionsFieldOperator.GreaterThan:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Gt("Quantity", value));
                    break;
                case UserReactionsFieldOperator.GreaterThanEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Ge("Quantity", value));
                    break;
                case UserReactionsFieldOperator.Between:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Between("Quantity", min, max));
                    break;
                case UserReactionsFieldOperator.NotBetween:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Not(z => z.Between("Quantity", min, max)));
                    break;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content items having the specified reactions.");
        }
    }
}