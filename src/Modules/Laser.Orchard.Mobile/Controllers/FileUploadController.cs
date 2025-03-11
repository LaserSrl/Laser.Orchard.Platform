using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class FileUploadController : Controller {
        private readonly ShellSettings _shellSettings;

        public FileUploadController(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        [Authorize]
        [HttpPost]
        public JsonResult UploadConfiguration() {
            if (Request != null) {
                if (Request.Form["conf"] != null) {
                    string firebase_conf_folder = HostingEnvironment.MapPath(
                    string.Format("~/App_Data/Sites/{0}/PushConfiguration/",
                        _shellSettings.Name));
                    if (!System.IO.Directory.Exists(firebase_conf_folder)) {
                        System.IO.Directory.CreateDirectory(firebase_conf_folder);
                    }

                    var confFileName = System.IO.Path.Combine(firebase_conf_folder, "fbpconfiguration.json");
                    System.IO.File.WriteAllText(confFileName, Request.Form["conf"]);

                    return Json(new {
                        Success = true,
                        Filename = System.IO.Path.GetFileName(confFileName)
                    });
                }
            }

            return Json(new {
                Success = false
            });
        }
    }
}