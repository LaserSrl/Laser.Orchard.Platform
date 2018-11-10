using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using Orchard.Core.Common.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Projections {
    public class OwnerIdFilter : IFilterProvider {

        public OwnerIdFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Owner", T("Owner"), T("Owner"))
                .Element("OwnerID", T("Owner ID"), T("Search for Content Items associated with a owner ID."),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "OwnerIdForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            if (context.State != null)
                if (context.State.OwnerId != null && context.State.OwnerId != "") {
                    List<int> ownersId = new List<int>();
                    string[] owns = context.State.OwnerId.Value.ToString().Split(',');
                    foreach (string own in owns) {
                        var ownerId = 0;
                        if (int.TryParse(own, out ownerId)) {
                            ownersId.Add(ownerId);
                        }
                    }

                    context.Query = query.Where(x => x.ContentPartRecord<CommonPartRecord>(),
                        x => x.In("OwnerId", ownersId.ToArray()));
                }
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            if (context.State.OwnerId != null && context.State.OwnerId != "")
                return T("Content Items with owner id equal to: {0}.", context.State.OwnerId);
            else
                return T("No owner Id found");
        }
    }
}