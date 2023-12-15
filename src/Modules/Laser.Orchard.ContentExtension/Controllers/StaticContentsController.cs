using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Laser.Orchard.ContentExtension.Controllers
{
    [OrchardFeature("Laser.Orchard.ContentExtension.StaticContents")]
    public class StaticContentsController : Controller
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IStaticContentsService _staticContentsService;

        public StaticContentsController(IStorageProvider storageProvider, IStaticContentsService staticContentsService)
        {
            _storageProvider = storageProvider;
            _staticContentsService = staticContentsService;
        }

        public FileResult Display(string path)
        {
            string staticContentsFolder = _staticContentsService.GetBaseFolder();
            string filePath = Path.Combine(staticContentsFolder, path.Replace("/", "\\"));
            if (!System.IO.File.Exists(filePath)) { throw new HttpException(404, "Not found"); }
            return File(filePath, MimeMapping.GetMimeMapping(filePath));
        }
    }
}