using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;
using System.Web.Hosting;

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
