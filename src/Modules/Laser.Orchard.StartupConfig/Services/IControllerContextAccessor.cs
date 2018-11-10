using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IControllerContextAccessor: IDependency {
        ControllerContext Context {get;set;}
    }
}
