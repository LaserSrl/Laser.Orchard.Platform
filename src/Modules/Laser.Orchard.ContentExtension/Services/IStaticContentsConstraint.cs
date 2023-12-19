using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Laser.Orchard.ContentExtension.Services
{
    public interface IStaticContentsConstraint : IRouteConstraint, ISingletonDependency
    {

    }
}
