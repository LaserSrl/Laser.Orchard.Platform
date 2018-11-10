using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Projections;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FilterEditors;
using Form = Orchard.Projections.FilterEditors.Forms;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway.Projections {
    public class PhonePrefixFilter : IFilterProvider {

        public PhonePrefixFilter(StringFilterEditor stringFilterEditor) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("CommunicationContacts", T("Communication Contacts"), T("Communication Contacts"))
                .Element("PhonePrefix", T("Telephone Prefix"), T("Contacts with the specified telephone prefix"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "StringFilter"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;

            string subquery = @"SELECT contact.SmsContactPartRecord_Id as contactId
                                FROM Laser.Orchard.CommunicationGateway.Models.CommunicationSmsRecord AS contact ";

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            var op = (Form.StringOperator)Enum.Parse(typeof(Form.StringOperator), Convert.ToString(context.State.Operator));

            switch (op) {
                case Form.StringOperator.Equals:
                    subquery += "WHERE contact.Prefix = :prefix";
                    parameters.Add("prefix", Convert.ToString(context.State.Value));
                    break;
                case Form.StringOperator.NotEquals:
                    subquery += "WHERE contact.Prefix != :prefix";
                    parameters.Add("prefix", Convert.ToString(context.State.Value));
                    break;
                case Form.StringOperator.Contains:
                    subquery += "WHERE contact.Prefix LIKE :prefix";
                    parameters.Add("prefix", "%" + Convert.ToString(context.State.Value) + "%");
                    break;
                case Form.StringOperator.ContainsAny:
                    if (string.IsNullOrEmpty((string)context.State.Value))
                        subquery += "Id = 0";
                    else {
                        int i = 0;
                        string[] values = Convert.ToString(context.State.Value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                        subquery += "WHERE ";
                
                        foreach (var value in values) {
                            subquery += String.Format("contact.Prefix LIKE :prefix{0} ", i);
                            parameters.Add("prefix" + i, "%" + value + "%");
                            i++;
                            if (i < values.Length)
                                subquery += "OR ";
                        }
                    }
                    break;
                case Form.StringOperator.ContainsAll:
                    if (string.IsNullOrEmpty((string)context.State.Value))
                        subquery += "Id = 0";
                    else {
                        int i = 0;
                        string[] values = Convert.ToString(context.State.Value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        subquery += "WHERE ";

                        foreach (var value in values) {
                            subquery += String.Format("contact.Prefix LIKE :prefix{0} ", i);
                            parameters.Add("prefix" + i, "%" + value + "%");
                            i++;
                            if (i < values.Length)
                                subquery += "AND ";
                        }
                    }
                    break;
                case Form.StringOperator.NotContains:
                    subquery += "WHERE contact.Prefix NOT LIKE :prefix";
                    parameters.Add("prefix", "%" + Convert.ToString(context.State.Value) + "%");
                    break;
                case Form.StringOperator.Starts:
                    subquery += "WHERE contact.Prefix LIKE :prefix";
                    parameters.Add("prefix", Convert.ToString(context.State.Value) + "%");
                    break;
                case Form.StringOperator.NotStarts:
                    subquery += "WHERE contact.Prefix NOT LIKE :prefix";
                    parameters.Add("prefix", Convert.ToString(context.State.Value) + "%");
                    break;
                case Form.StringOperator.Ends:
                    subquery += "WHERE contact.Prefix LIKE :prefix";
                    parameters.Add("prefix", "%" + Convert.ToString(context.State.Value));
                    break;
                case Form.StringOperator.NotEnds:
                    subquery += "WHERE contact.Prefix NOT LIKE :prefix";
                    parameters.Add("prefix", "%" + Convert.ToString(context.State.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            context.Query = query.Where(x => x.ContentPartRecord<CommunicationContactPartRecord>(), x => x.InSubquery("Id", subquery, parameters));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Filter contacts with the specified telephone prefix.");
        }
    }
}