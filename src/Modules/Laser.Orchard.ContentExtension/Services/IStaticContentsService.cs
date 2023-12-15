using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.ContentExtension.Services
{
    public interface IStaticContentsService : ISingletonDependency
    {
        string GetBaseFolder();
    }
}
