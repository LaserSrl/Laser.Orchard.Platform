using System.Collections.Generic;

namespace Laser.Orchard.Reporting {
    public static class Extensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }
    }
}