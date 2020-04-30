using Laser.Orchard.ContentExtension.Models;
using NHibernate;
using Orchard;
using System.Collections;
using System.Collections.Generic;

namespace Laser.Orchard.ContentExtension.Services {
    public interface IDynamicProjectionService : IDependency {
        int GetCount(IQuery query);
        IEnumerable GetResults(IQuery query, int skip = 0, int count = 0);
        IEnumerable<DynamicProjectionPart> GetPartsForMenu();
    }
}
