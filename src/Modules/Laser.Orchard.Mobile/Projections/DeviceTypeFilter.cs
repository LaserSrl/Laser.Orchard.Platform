using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.Mobile.Projections {
    public class DeviceTypeFilter : IFilterProvider {
        public Localizer T { get; set; }

        public DeviceTypeFilter() {
		    T = NullLocalizer.Instance;
        }

        public void Describe(DescribeFilterContext describe) {
            describe.For("CommunicationContacts", T("Communication Contacts"), T("Communication Contacts"))
                    .Element("DeviceType", T("Device Type"), T("The type of the device the contact registered with (i.e. Android, Apple, Windows)."),
                        ApplyFilter,
                        DisplayFilter,
                        "DeviceTypeForm");
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;

            string subquery = @"SELECT contact.MobileContactPartRecord_Id as contactId
                                FROM Laser.Orchard.Mobile.Models.PushNotificationRecord AS contact 
                                WHERE Device = :device";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("device", Convert.ToString(context.State.DeviceType));

            context.Query = query.Where(x => x.ContentPartRecord<CommunicationContactPartRecord>(), x => x.InSubquery("Id", subquery, parameters));
        }

        public LocalizedString DisplayFilter(dynamic context) { return T("Filter by the type of the device the contact registered with (i.e. Android, Apple, Windows)."); }

    }
}