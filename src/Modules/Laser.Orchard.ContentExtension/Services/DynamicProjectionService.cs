using NHibernate;
using NHibernate.Transform;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.ContentExtension.Services {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionService : IDynamicProjectionService {
        private readonly IOrchardServices _orchardServices;

        public DynamicProjectionService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public int GetCount(IQuery query) {
            var count = 0;
            int.TryParse(query.UniqueResult().ToString(), out count);
            return count;
        }

        public IEnumerable GetResults(IQuery query, int skip = 0, int count = 0) {
            IQuery hqlQuery;

            if (skip != 0) {
                query.SetFirstResult(skip);
            }
            if (count != 0 && count != Int32.MaxValue) {
                query.SetMaxResults(count);
            }
            hqlQuery = query.SetResultTransformer(Transformers.AliasToEntityMap);
            return hqlQuery.Enumerable();
        }
    }
}