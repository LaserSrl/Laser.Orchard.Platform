using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public ActionResult Display(string path)
        {
            string staticContentsFolder = _staticContentsService.GetBaseFolder();
            string filePath = Path.Combine(staticContentsFolder, path.Replace("/", "\\"));
            if (!_staticContentsService.StaticContentIsAllowed(filePath) || !System.IO.File.Exists(filePath)) { return HttpNotFound(); }
            if (false) //TODO: To Implement Content negotiation
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable);

            var mime = MimeMapping.GetMimeMapping(filePath);
            var mimeFile = filePath + ".mime";
            if (System.IO.File.Exists(mimeFile))
            {
                mime = System.IO.File.ReadAllText(mimeFile);
            }
            return File(filePath, mime);
        }
    }
}