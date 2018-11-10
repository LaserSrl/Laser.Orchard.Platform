using Laser.Orchard.StartupConfig.Projections;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System.Collections.Generic;
using OrchardProjections = Orchard.Projections;

namespace Laser.Orchard.UserProfiler.Projections {

    public class UserTrackingQueryFilter : OrchardProjections.Services.IFilterProvider {
        public Localizer T { get; set; }

        public UserTrackingQueryFilter() {
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Profilling", T("User Profilling"), T("User Profilling"))
                .Element("User Tracking Filter", T("User Tracking Filter"), T("Filter for a specific tracking element."),
                    ApplyFilter,
                    DisplayFilter,
                    "UserTrackingFilterForm"
                );
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("User who interact with specific tag on contentitem.");
        }

        public void ApplyFilter(FilterContext context) {
            var Tag = (string)context.State.UserProfillingTag;
            if (string.IsNullOrEmpty(Tag))
                Tag = (string)context.State.UserProfillinglist;

            if (!string.IsNullOrEmpty(Tag)) {
                string subquery = @"select distinct contact.Id as contactId
                from Laser.Orchard.CommunicationGateway.Models.CommunicationContactPartRecord as contact,
                Laser.Orchard.UserProfiler.Models.UserProfilingSummaryRecord as usertrack
                where usertrack.UserProfilingPartRecord.Id = contact.UserPartRecord_Id
                and usertrack.SourceType = 'Tag'
                and usertrack.Text = :tagtext";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("tagtext", Tag);

                context.Query.Where(a => a.Named("ci"), x => x.InSubquery("Id", subquery, parameters));
            }
        }
    }
}