using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    public interface IMultiStepAccountApiController : IDependency {
        // This interface will be used to discriminate ApiControllers for multi-step authentication
    }
}
