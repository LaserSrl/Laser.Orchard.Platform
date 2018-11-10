using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Commons.Services {
    /// <summary>
    /// Implementations of this interface will be injected in the Generator.
    /// </summary>
    public interface IDumperService : IDependency {

        /// <summary>
        /// Implementations of this method are required wherever a part or field
        /// would add a list to the result of an object dump
        /// </summary>
        /// <param name="context"></param>
        void DumpList(DumperServiceContext context);
    }
}
