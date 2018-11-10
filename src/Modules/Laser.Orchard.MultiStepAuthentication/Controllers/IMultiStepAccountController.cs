using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    public interface IMultiStepAccountController : IDependency {
        // This interface will be used to discriminate controllers for multi-step authentication
    }
}
