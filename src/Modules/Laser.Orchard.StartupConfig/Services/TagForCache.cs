using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface ITagForCache : IDependency {
        void Add(int id);
        IEnumerable<int> Get();
    }
    public class TagForCache:ITagForCache {
        private readonly Collection<int> _itemIds = new Collection<int>();
        public void Add(int id) {
            _itemIds.Add(id);
        }
        public IEnumerable<int> Get() {
            return _itemIds.Distinct();
        }
    }
}