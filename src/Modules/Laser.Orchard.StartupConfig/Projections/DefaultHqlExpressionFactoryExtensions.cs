using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.StartupConfig.Projections {
    public static class DefaultHqlExpressionFactoryExtensions {
        private static void InSubquery(this IHqlExpressionFactory hqlExpressionFactory, string propertyName, string subquery) {
            var aux = (hqlExpressionFactory as DefaultHqlExpressionFactory);
            var crit = InSubquery(propertyName, subquery);
            var property = typeof(DefaultHqlExpressionFactory).GetProperty("Criterion");
            property.SetValue(aux, crit);
        }

        /// <summary>Search inside the results of a parameterized query.</summary>
        /// <param name="propertyName">The property to compare.</param>
        /// <param name="subquery">The subquery to evaluate. It can contain parameters.
        /// A parameter is expressed as a word starting with ':' and no other character before.
        /// (i.e. SELECT * FROM table WHERE column = :param)</param>
        /// <param name="parameters">A dictionary containing the parameter names (without ':') as keys and their values.</param>
        public static void InSubquery(this IHqlExpressionFactory hqlExpressionFactory, string propertyName, string subquery, Dictionary<string, object> parameters) {
            string subqueryWithParameters = "";

            Regex re = new Regex(@"(?<![^\s]):[^\s]+");
            subqueryWithParameters = re.Replace(subquery, x => {
                object param;

                if (parameters.TryGetValue(x.ToString().TrimStart(':'), out param)) {
                    var typeCode = Type.GetTypeCode(param.GetType());
                    switch (typeCode) {
                        case TypeCode.String:
                        case TypeCode.DateTime:
                            return HqlRestrictions.FormatValue(param);
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                            return FormatNumber(param);
                        default:
                            return "";
                    }
                }

                return "";
            });

            InSubquery(hqlExpressionFactory, propertyName, subqueryWithParameters);
        }

        private static IHqlCriterion InSubquery(string propertyName, string subquery) {
            if (string.IsNullOrWhiteSpace(subquery)) {
                throw new ArgumentException("Subquery can't be empty", "subquery");
            }
            return new BinaryExpression("in", propertyName, "(" + subquery + ")");
        }

        private static string FormatNumber(object value) {
            decimal num;
            if (Decimal.TryParse(value.ToString(), out num))
                return HqlRestrictions.FormatValue(value);
            else
                return "";
        }
    }
}