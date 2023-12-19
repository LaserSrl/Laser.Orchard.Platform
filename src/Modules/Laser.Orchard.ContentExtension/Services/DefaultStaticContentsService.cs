using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.ContentManagement;
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
        private readonly IWorkContextAccessor _workcontext;

        public DefaultStaticContentsService(IStorageProvider storageProvider, IWorkContextAccessor workcontext)
        {
            _storageProvider = storageProvider;
            _workcontext = workcontext;
        }

        public string GetBaseFolder()
        {
            string staticContentsFolder = _storageProvider.GetPublicUrl("_StaticContents");
            string folder = HostingEnvironment.MapPath(staticContentsFolder);
            return folder;
        }

        public bool StaticContentIsAllowed(string filePath)
        {
            var settings = _workcontext.GetContext().CurrentSite.As<MediaLibrarySettingsPart>();
            return settings.IsFileAllowed(filePath);
        }
    }
}
