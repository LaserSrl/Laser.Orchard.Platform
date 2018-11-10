using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using System;

namespace Laser.Orchard.CommunicationGateway.Projections {

    public interface IFilterProvider : IEventHandler {

        void Describe(dynamic describe);
    }

    public class ExcludeMasterContact : IFilterProvider {

        public ExcludeMasterContact() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("CommunicationContacts", T("Communication Contacts"), T("Communication Contacts"))
                .Element("ExcludeMasterContact", T("Exclude Master Contact"), T("Filter for real contacts (master contact will be excluded)"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    null
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            context.Query = query.Where(x => x.ContentPartRecord<CommunicationContactPartRecord>(), x => x.Eq("Master", false));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Filter only real contacts (master contact will be excluded)");
        }
    }
}