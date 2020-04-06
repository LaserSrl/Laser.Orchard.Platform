using NHibernate;
using NHibernate.Transform;
using Orchard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.ContentExtension.Services {
    public class DynamicProjectionService : IDynamicProjectionService {
        private readonly IOrchardServices _orchardServices;

        public DynamicProjectionService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public IEnumerable GetResults(string query, int skip = 0, int count = 0) {
            IQuery hqlQuery;
            string[] aliases;
            bool cacheable = true;
            var hql = _orchardServices.TransactionManager.GetSession().CreateQuery(query).SetCacheable(cacheable);
            if (skip != 0) {
                hql.SetFirstResult(skip);
            }
            if (count != 0 && count != Int32.MaxValue) {
                hql.SetMaxResults(count);
            }
            var startsWithSelect = new Regex(@"^select\s", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (startsWithSelect.IsMatch(query)) {
                hqlQuery = hql.SetResultTransformer(Transformers.AliasToEntityMap);
            }
            else {
                throw new Exception("Query should starts with \"select\" keyword.\r\nQuery is:\r\n" + query);
            }
            return hqlQuery.Enumerable();
        }
    }
}