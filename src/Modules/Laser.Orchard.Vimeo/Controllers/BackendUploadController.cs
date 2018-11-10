using Laser.Orchard.Vimeo.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo.Controllers {
    [Admin, Themed(false)]
    public class BackendUploadController : Controller {

        public BackendUploadController() {

        }

        public ActionResult Index(string folderPath, string type) {
            
            var viewModel = new BackendUploadViewModel {
                FolderPath = folderPath,
                Type = type
            };

            return View(viewModel);
        }
    }
}