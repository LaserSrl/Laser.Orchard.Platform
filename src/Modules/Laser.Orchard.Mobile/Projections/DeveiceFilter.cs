using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using Orchard.Projections.Descriptors.Filter;
using Laser.Orchard.Mobile.Models;
using System.Reflection;
using System.Collections.Generic;


namespace Laser.Orchard.Mobile.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(DescribeFilterContext describe);
    }

    public class DeveiceFilter : IFilterProvider {
        public Localizer T { get; set; }
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public DeveiceFilter(IWorkContextAccessor workContextAccessor) {
            T = NullLocalizer.Instance;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
        }

        public void Describe(DescribeFilterContext describe) {
            describe
                    .For("User", T("User Device"), T("User Device"))
                    .Element("UserDEvice", T("User Device"), T("Filter for device associated to User"),
                             ApplyFilter,
                             DisplayFilter
                             );

        }

        public void ApplyFilter(FilterContext context) {
            #region query
          
            IHqlQuery query = context.Query;
       
            query.Join(alias => alias.ContentItem());
            var defaultHqlQuery = query as DefaultHqlQuery;
            var fiJoins = typeof(DefaultHqlQuery).GetField("_joins", BindingFlags.Instance | BindingFlags.NonPublic);
            var joins = fiJoins.GetValue(defaultHqlQuery) as List<Tuple<IAlias, Join>>;
            joins.Add(new Tuple<IAlias, Join>(new Alias("Laser.Orchard.Mobile.Models"), new Join("UserDeviceRecord", "UserDevice", ",")));

            context.Query = query.Where(
            alias => alias.Named("UserDevice"), predicate => predicate.EqProperty("UserPartRecord", "ci"));
         
            #endregion
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Content with userpart");
        }

    }
}