using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using Orchard.Core.Common.Models;
using Orchard.Users.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Projections {
    public class OwnerEmailFilter : IFilterProvider {
        private readonly IContentManager _contentManager;
        public Localizer T { get; set; }

        public OwnerEmailFilter(IContentManager contentManager) {
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic describe) {
            describe.For("Owner", T("Owner"), T("Owner"))
                .Element("OwnerEmail", T("Owner Email"), T("Search for Content Items associated with a owner email."),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "OwnerStringForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            if (context.State != null && context.State.Value != null && context.State.Value != "") {
                string op = context.State.Operator;
                string val = context.State.Value;
                IEnumerable<UserPart> owners = getOwners(op, val);
                List<int> ownersId = new List<int>();
                foreach (UserPart owner in owners) {
                    ownersId.Add(owner.Id);
                }
                context.Query = query.Where(x => x.ContentPartRecord<CommonPartRecord>(),
                    x => x.In("OwnerId", ownersId.ToArray()));

            }
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            if (context.State.Value != null && context.State.Value != "")
                return T("Content Items with owner: {0}.", context.State.Value);
            else
                return T("No owner email found");
        }

        private IEnumerable<UserPart> getOwners(string op, string stateValue) {
            List<UserPart> owners = new List<UserPart>();
            switch (op) {
                case "Equals":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email.Equals(stateValue)).List();
                case "NotEquals":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => !u.Email.Equals(stateValue)).List();
                case "Contains":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email.Contains(stateValue)).List();
                case "ContainsAny":
                    string[] ownList = stateValue.Split(',');
                    foreach (string owner in ownList) {
                        owners.AddRange(_contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email.Equals(owner)).List());
                    }
                    return owners;
                case "Starts":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email.StartsWith(stateValue, StringComparison.OrdinalIgnoreCase)).List();
                case "NotStarts":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => !u.Email.StartsWith(stateValue, StringComparison.OrdinalIgnoreCase)).List();
                case "Ends":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email.EndsWith(stateValue, StringComparison.OrdinalIgnoreCase)).List();
                case "NotEnds":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => !u.Email.EndsWith(stateValue, StringComparison.OrdinalIgnoreCase)).List();
                case "NotContains":
                    return _contentManager.Query<UserPart, UserPartRecord>().Where(u => !u.Email.Contains(stateValue)).List();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}