using Laser.Orchard.StartupConfig.ShortCodes;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.StartupConfig.ShortCodes.Abstractions {
    public interface IShortCodeProvider : IDependency {
        Descriptor Describe();
        void Evaluate(EvaluateContext context);
    }
}
