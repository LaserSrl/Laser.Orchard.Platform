using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Orchard;

namespace Laser.Orchard.PrivateMedia.Controllers {
    public class GetMediaController : Controller {
      //  private readonly IStorageProvider _storageProvider;
        private readonly IOrchardServices _orchardServices;
        public GetMediaController(      IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        [OutputCache(NoStore = true, Duration = 0)]//, VaryByParam = "filename"
        public ActionResult Image(string filename) {
            if (!_orchardServices.Authorizer.Authorize(PrivateMediaPermissions.AccessAllPrivateMedia)) {
                return null;
            }
          //  WebImage img;
            lock (String.Intern(filename)) {
                var _storagePath = HostingEnvironment.IsHosted
                ? HostingEnvironment.MapPath("~/Media/") ?? ""
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
                filename = filename.Substring(filename.IndexOf("/Media/") + 7);
                filename = filename.Replace("/", @"\");
                filename = _storagePath + filename;
           //    img = new WebImage(filename);

            }
            return base.File(filename, MimeMapping.GetMimeMapping(filename));
            // return new ImageResult(new MemoryStream(img.GetBytes()), "image/"+img.ImageFormat);// "image /jpeg");
        }
    }

    //[OrchardFeature("Laser.Orchard.StartupConfig.PrivateMedia")]
    //public class ImageResult : ActionResult {
    //    public ImageResult(Stream imageStream, string contentType) {
    //        if (imageStream == null)
    //            throw new ArgumentNullException("imageStream");
    //        if (contentType == null)
    //            throw new ArgumentNullException("contentType");

    //        this.ImageStream = imageStream;
    //        this.ContentType = contentType;
    //    }

    //    public Stream ImageStream { get; set; }
    //    public string ContentType { get; set; }

    //    public override void ExecuteResult(ControllerContext context) {
    //        if (context == null)
    //            throw new ArgumentNullException("context");

    //        var response = context.HttpContext.Response;

    //        response.ContentType = this.ContentType;

    //        byte[] buffer = new byte[4096];
    //        while (true) {
    //            int read = this.ImageStream.Read(buffer, 0, buffer.Length);
    //            if (read == 0)
    //                break;

    //            response.OutputStream.Write(buffer, 0, read);
    //        }

    //        response.End();
    //    }
    //}
}