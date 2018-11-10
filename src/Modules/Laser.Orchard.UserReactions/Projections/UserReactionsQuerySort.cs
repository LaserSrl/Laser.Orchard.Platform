using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQuerySort : ISortCriterionProvider {
        public Localizer T { get; set; }

        public UserReactionsQuerySort() {
            T = NullLocalizer.Instance;
        }
        
        public void Describe(DescribeSortCriterionContext describe) {
            describe.For("Reactions", T("Reactions"), T("Reactions"))
                .Element("Reaction", T("Reaction on content"), T("Reaction on content"),
                    ApplySort,
                    DisplaySort,
                    "ReactionsSortForm"
                );
        }

        private void ApplySort(SortCriterionContext context) {
            bool ascending = Convert.ToBoolean(context.State.Sort);
            int reactionTypeId = Convert.ToInt32(context.State.Reaction);
            context.Query = context.Query.Join(
                x => x.ContentPartRecord<UserReactionsPartRecord>()
                .Property("Reactions", "reactionsSummarySort")
                .Property("UserReactionsTypesRecord", "reactionTypeSort"));
            context.Query.Where(a => a.Named("reactionTypeSort"), x => x.Eq("Id", reactionTypeId));

            if (ascending) {
                context.Query.OrderBy(a => a.Named("reactionsSummarySort"), x => x.Asc("Quantity"));
            }
            else {
                context.Query.OrderBy(a => a.Named("reactionsSummarySort"), x => x.Desc("Quantity"));
            }
        }

        private LocalizedString DisplaySort(SortCriterionContext context) {
            bool ascending = Convert.ToBoolean(context.State.Sort);
            string orderedField = T("Reactions number").Text;

            if (ascending)
                return T("Ordered by {0}, ascending", orderedField);
            else
                return T("Ordered by {0}, descending", orderedField);
        }
    }
}