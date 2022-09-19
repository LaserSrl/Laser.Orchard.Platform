using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.WebServices.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Security;
using System.Web.Mvc;

namespace Laser.Orchard.Generator.Controllers {
    public class WebApiController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IWebApiService _webApiServices;
        private readonly IContentSerializationServices _contentSerializationServices;

        //
        // GET: /Json/
        public WebApiController(IOrchardServices orchardServices,
            IWebApiService webApiSerices,
            IContentSerializationServices contentSerializationServices) {
            _orchardServices = orchardServices;
            _webApiServices = webApiSerices;
            _contentSerializationServices = contentSerializationServices;

        }

        public ILogger Logger { get; set; }

        [AlwaysAccessible]
        [OutputCache(NoStore = true, Duration = 0)] // do not cache generator calls
        public ActionResult Terms(string alias, int maxLevel = 10) {
            return _webApiServices.Terms(alias, maxLevel);
        }

        [AlwaysAccessible]
        [OutputCache(NoStore = true, Duration = 0)] // do not cache generator calls
        public ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10, string filter = "") {
            JObject json;

            IContent content;
            if (alias.ToLower() == "user+info" || alias.ToLower() == "user info") {
                #region [ Richiesta dati di uno user ]
                #region utente validato

                //         item = currentUser.ContentItem;
                content = _orchardServices.ContentManager.Get(2);
                json = _contentSerializationServices.GetJson(content, page, pageSize);
                //_contentSerializationServices.NormalizeSingleProperty(json);
                return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");

                #endregion utente validato

                #endregion

            } else {
                return _webApiServices.Display(alias, page, pageSize, maxLevel, filter);
            }
        }

    }
}