using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Taxonomies.Models;

namespace Laser.Orchard.WebServices.Controllers {
    public class TermsController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IUtilsServices _utilsServices;
        public TermsController(IContentManager contentManager, IUtilsServices utilsServices) {
            _contentManager = contentManager;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpGet]
        public JsonResult GetIconsIds() {
            var items = _contentManager.Query<TermPart, TermPartRecord>().List();
            var listIconIds = new List<int>();
            try {
                foreach (dynamic item in items) {
                    if (item.Icon != null && ((int[])item.Icon.Ids).Length > 0) {
                        listIconIds = listIconIds.Union((int[])item.Icon.Ids).ToList();
                    }
                    else if (item.Icon == null) {
                        var error = _utilsServices.GetResponse(StartupConfig.ViewModels.ResponseType.None);
                        error.ErrorCode = StartupConfig.ViewModels.ErrorCode.GenericError;
                        error.Data = T("Missing Icon field in Term Part.").Text;
                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }
                // verifica che le icon non siano state eliminate
                listIconIds = _contentManager.Query().ForContentItems(listIconIds).List().Select(x => x.Id).ToList<int>();
            }
            catch (Exception ex) {
                var error = _utilsServices.GetResponse(StartupConfig.ViewModels.ResponseType.None);
                error.ErrorCode = StartupConfig.ViewModels.ErrorCode.GenericError;
                error.Data = ex.Message;
                return Json(error, JsonRequestBehavior.AllowGet);
            }
            return Json(listIconIds.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}