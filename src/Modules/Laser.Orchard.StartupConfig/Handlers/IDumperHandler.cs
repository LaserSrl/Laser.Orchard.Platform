using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.StartupConfig.Handlers {
    public interface IDumperHandler : IDependency {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">Item to set values to</param>
        /// <param name="listProperty">Array of segments of a property, usually [part_name, field_name]</param>
        /// <param name="value">Value to set on item property</param>
        /// <returns>True if item was handled, false otherwise</returns>
        void StoreLikeDynamic(ContentItem item, string[] listProperty, object value);
    }
}
