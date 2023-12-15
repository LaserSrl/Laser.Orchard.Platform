using Orchard;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;

namespace Laser.Orchard.ContentExtension.Services
{
    [OrchardFeature("Laser.Orchard.ContentExtension.StaticContents")]
    public class DefaultStaticContentsService : IStaticContentsService
    {
        private readonly IStorageProvider _storageProvider;

        public DefaultStaticContentsService(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public string GetBaseFolder()
        {
            string staticContentsFolder = _storageProvider.GetPublicUrl("__StaticContents");
            string folder = HostingEnvironment.MapPath(staticContentsFolder);
            return folder;
        }
    }
}
