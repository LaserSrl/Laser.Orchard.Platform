using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.Vimeo.Services;
using Newtonsoft.Json;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace Laser.Orchard.Vimeo.Controllers {
    [WebApiKeyFilter(true)]
    public class VimeoUploadAPIController : ApiController {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IVimeoUploadServices _vimeoUploadServices;
        private readonly IUtilsServices _utilsServices;

        private VimeoUploadController _uploadController;

        public VimeoUploadAPIController(IVimeoUploadServices vimeoUploadServices, IUtilsServices utilsServices) {
            _vimeoUploadServices = vimeoUploadServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            _uploadController = new VimeoUploadController(_vimeoUploadServices, _utilsServices);
        }

        //NOTE: The methods here are pretty much wrappers for those in VimeoUploadController

        [System.Web.Mvc.HttpPost]
        public Response TryStartUpload(int fileSize) {
            JsonResult res = (JsonResult)_uploadController.TryStartUpload(fileSize);
            return (Response)res.Data;// JsonConvert.DeserializeObject<Response>(res.ToString());
        }

        [System.Web.Mvc.HttpPost]
        public Response FinishUpload(int mediaPartId) {
            JsonResult res = (JsonResult)_uploadController.FinishUpload(mediaPartId);
            return (Response)res.Data;
        }

        [System.Web.Mvc.HttpPost]
        public Response ErrorHandler() {
            string msgJson = new StreamReader(HttpContext.Current.Request.GetBufferedInputStream()).ReadToEnd();
            return _uploadController.ErrorHandler(msgJson);
        }
    }
}