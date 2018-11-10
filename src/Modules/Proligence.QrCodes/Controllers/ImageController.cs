namespace Proligence.QrCodes.Controllers {
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;

    using Gma.QrCodeNet.Encoding;
    using Gma.QrCodeNet.Encoding.Windows.Render;
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.Environment.Configuration;
    using Orchard.FileSystems.Media;
    using Orchard.Localization;
    using Orchard.Themes;
    using Orchard.UI.Admin;

    using Proligence.QrCodes.Models;

    [Themed(false)]
    public class ImageController : Controller {

        private readonly IStorageProvider _storageProvider;
        private readonly ShellSettings _settings;
        public ImageController(IStorageProvider storageProvider, ShellSettings settings) {
            _storageProvider = storageProvider;
            _settings = settings;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Render(int id) {
            var _publicMediaPath = _storageProvider.GetPublicUrl("qrcode");
            var uncache = "";
            var mediaPath = HostingEnvironment.IsHosted
                                  ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                  : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
            mediaPath = Path.Combine(mediaPath, _settings.Name);
            string filepath = string.Format(@"{0}\qrcode\qrcode_{1}.png", mediaPath, id.ToString());
            if (System.IO.File.Exists(filepath)) {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan diff = System.IO.File.GetLastWriteTime(filepath) - origin;
                uncache = "?" + Math.Floor(diff.TotalSeconds).ToString();
            }
            return Content(string.Format(_publicMediaPath + "/qrcode_{0}.png{1}", id.ToString(), uncache));
        }
    }
}
