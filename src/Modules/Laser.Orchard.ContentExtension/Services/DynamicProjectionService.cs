using Laser.Orchard.ContentExtension.Models;
using NHibernate;
using NHibernate.Transform;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laser.Orchard.ContentExtension.Services {
    [OrchardFeature("Laser.Orchard.ContentExtension.DynamicProjection")]
    public class DynamicProjectionService : IDynamicProjectionService {
        private readonly IContentManager _contentManager;

        public DynamicProjectionService(
            IContentManager contentManager) {

            _contentManager = contentManager;
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
            return hqlQuery.List();
        }

        private IEnumerable<DynamicProjectionPart> _partsForMenu;
        public IEnumerable<DynamicProjectionPart> GetPartsForMenu() {
            if (_partsForMenu == null) {
                _partsForMenu = _contentManager
                    .Query<DynamicProjectionPart, DynamicProjectionPartRecord>()
                    .Where(x => x.OnAdminMenu).List();
                
            }
            return _partsForMenu;
        }
    }
}