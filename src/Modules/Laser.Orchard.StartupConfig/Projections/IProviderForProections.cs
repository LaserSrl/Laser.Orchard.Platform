using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Events;

namespace Laser.Orchard.StartupConfig.Projections {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

}
