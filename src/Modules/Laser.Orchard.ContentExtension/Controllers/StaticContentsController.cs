using Laser.Orchard.ContentExtension.Services;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
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
            if (!_staticContentsService.StaticContentIsAllowed(filePath) ||
                !System.IO.File.Exists(filePath)) { return HttpNotFound(); }

            /*We don't want to expose .mime files that we use here by convention to explicitly define mime types*/
            if (filePath.EndsWith("._mime", StringComparison.InvariantCultureIgnoreCase) &&
                System.IO.File.Exists(filePath.Substring(0, filePath.Length - 6)))
            {
                return HttpNotFound();
            }

            var mime = MimeMapping.GetMimeMapping(filePath);
            var mimeFile = filePath + "._mime";
            if (System.IO.File.Exists(mimeFile))
            {
                if (System.IO.File.ReadAllText(mimeFile).Count(f => f == '/') == 1)
                {
                    mime = System.IO.File.ReadAllText(mimeFile);
                }
            }
            if (!IsAcceptable(Request.Headers["Accept"], mime)) //TODO: To Implement Content negotiation
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable);

            return File(filePath, mime);
        }

        private bool IsAcceptable(string accept, string contentType)
        {
            var acceptItems = Request.Headers["Accept"]?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new[] { "*/*" };
            var mimeSegments = contentType.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (mimeSegments.Length != 2) { return true; }
            var responseMime = new Mime {
                Type = mimeSegments[0].Trim(),
                SubType = mimeSegments[1].Trim()
            };

            var requestAccept = acceptItems.Select(x => {
                var acceptSegments = x.Split(new[] { '/', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var Qvalue = 1.0M;
                if (acceptSegments.Length > 2)
                {
                    if (acceptSegments[2].Trim().StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal.TryParse(acceptSegments[2].Substring(2).Trim(), out Qvalue);
                    }
                }
                return new Mime {
                    Type = acceptSegments.Length > 0 ? acceptSegments[0].Trim() : "*",
                    SubType = acceptSegments.Length > 1 ? acceptSegments[1].Trim() : "*",
                    Quality = Qvalue
                };
            });

            return requestAccept.Any(x =>
                (x.Type.Equals("*") ||
                    x.Type.Equals(responseMime.Type, StringComparison.OrdinalIgnoreCase)) &&
                (x.SubType.Equals("*") ||
                    x.SubType.Equals(responseMime.SubType, StringComparison.OrdinalIgnoreCase)));
        }
    }

    internal class Mime
    {
        internal string Type { get; set; }
        internal string SubType { get; set; }
        internal decimal Quality { get; set; }
    }
}