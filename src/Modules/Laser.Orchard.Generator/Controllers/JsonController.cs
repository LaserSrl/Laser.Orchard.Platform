using Laser.Orchard.Commons.Enums;
using Laser.Orchard.Commons.Services;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.WebServices.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Laser.Orchard.Generator.Controllers
{

    public class JsonController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ShellSettings _shellSetting;
        private readonly IUtilsServices _utilsServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEnumerable<IDumperService> _dumperServices;
        private readonly IPolicyServices _policyServices;

        public ILogger Logger { get; set; }

        public JsonController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ShellSettings shellSetting,
            IUtilsServices utilsServices,
            ICsrfTokenHelper csrfTokenHelper,
            IAuthenticationService authenticationService,
            IEnumerable<IDumperService> dumperServices,
            IPolicyServices policyServices) {

            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _shellSetting = shellSetting;
            _utilsServices = utilsServices;
            _csrfTokenHelper = csrfTokenHelper;
            _authenticationService = authenticationService;
            _dumperServices = dumperServices;
            _policyServices = policyServices;

            Logger = NullLogger.Instance;
        }

        [AlwaysAccessible]
        [OutputCache(NoStore = true, Duration = 0)] // do not cache generator calls
        public ContentResult GetByAlias(
            string displayAlias, 
            SourceTypes sourceType = SourceTypes.ContentItem, 
            ResultTarget resultTarget = ResultTarget.Contents, 
            string mfilter = "", 
            int page = 1, 
            int pageSize = 10, 
            bool tinyResponse = true, 
            bool minified = false, 
            bool realformat = false, 
            int deeplevel = 10,
            string complexBehaviour = "") {

            //   Logger.Error("inizio"+DateTime.Now.ToString());
            IContent item = null;

            if (displayAlias.ToLower() == "user+info" || displayAlias.ToLower() == "user info") {
                // The call to this generator method is generally anonymous, but we still want to send out the json reflecting the structure of a user
                // so it can be mapped.
                // We are sending out the admin user, but this may end up being a security concern in some cases so:
                // TODO: figure out a way to not be sending out admin information here, since this call is anonymous.

                item = _orchardServices.ContentManager.Get(2);

            } else {
                var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .ForVersion(VersionOptions.Published)
                    .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();

                if (autoroutePart != null && autoroutePart.ContentItem != null) {
                    item = autoroutePart.ContentItem;
                } else {
                    new HttpException(404, ("Not found"));
                    return null;
                }
            }
            ContentResult cr = (ContentResult)GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, complexBehaviour.Split(','));
            //    Logger.Error("Fine:"+DateTime.Now.ToString());

            if (_orchardServices.WorkContext.CurrentSite.As<WebServiceSettingsPart>().LogWebservice) {
                Logger.Error(cr.Content.ToString());
            }
            return cr;
        }

        private ActionResult GetContent(
            IContent content,
            SourceTypes sourceType = SourceTypes.ContentItem,
            ResultTarget resultTarget = ResultTarget.Contents,
            string fieldspartsFilter = "",
            int page = 1,
            int pageSize = 10,
            bool tinyResponse = true,
            bool minified = false,
            bool realformat = false,
            int deeplevel = 10,
            string[] complexBehaviour = null) {

            var result = new ContentResult { ContentType = "application/json" };
            var jsonString = "{}";

            var _filterContentFieldsParts = fieldspartsFilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            XElement dump;
            XElement projectionDump = null;
            // il dump dell'oggetto principale non filtra per field
            ObjectDumper dumper = new ObjectDumper(deeplevel, null, false, tinyResponse, complexBehaviour);
            var sb = new StringBuilder();

            // verifico se l'oggetto è soggetto all'accettazione delle policies
            var policy = content.As<Policy.Models.PolicyPart>();
            if (policy != null) {
                if ((String.IsNullOrWhiteSpace(_orchardServices.WorkContext.HttpContext.Request.QueryString["v"]))) {// E' soggetto a privacy, quindi faccio sempre il redirect se manca il parametro in querystring v=
                    if (_policyServices.HasPendingPolicies(content.ContentItem) ?? false) { // se ha delle pending policies deve restituire le policy text, legate al contenuto, quindi non deve mai servire cache
                        var redirectUrl = String.Format("{0}{1}v={2}", _orchardServices.WorkContext.HttpContext.Request.RawUrl, (_orchardServices.WorkContext.HttpContext.Request.RawUrl.Contains("?") ? "&" : "?"), Guid.NewGuid());
                        _orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
                    } else {// se NON ha delle pending policies deve restituire un url non cacheato (quindi aggiungo v=),
                        var redirectUrl = String.Format("{0}{1}v={2}", _orchardServices.WorkContext.HttpContext.Request.RawUrl, (_orchardServices.WorkContext.HttpContext.Request.RawUrl.Contains("?") ? "&" : "?"), "cached-content");
                        _orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
                        //_orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
                    }
                    return null; // in entrambi i casi ritorno null come risultato della current request
                }
            }
            if (policy != null && (_policyServices.HasPendingPolicies(content.ContentItem) ?? false)) { // Se l'oggetto ha delle pending policies allora devo serivre la lista delle pending policies
                //policy.PendingPolicies
                sb.Insert(0, "{");
                sb.AppendFormat("\"n\": \"{0}\"", "Model");
                sb.AppendFormat(", \"v\": \"{0}\"", "VirtualContent");
                sb.Append(", \"m\": [{");
                sb.AppendFormat("\"n\": \"{0}\"", "VirtualId"); // Unused property for mobile mapper needs
                sb.AppendFormat(", \"v\": \"{0}\"", "0");
                sb.Append("}]");

                sb.Append(", \"l\":[");

                int i = 0;
                sb.Append("{");
                sb.AppendFormat("\"n\": \"{0}\"", "PendingPolicies");
                sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                sb.Append(", \"m\": [");

                foreach (var item in _policyServices.PendingPolicies(content.ContentItem)) {
                    if (i > 0) {
                        sb.Append(",");
                    }
                    sb.Append("{");
                    dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour);
                    projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                    JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                    sb.Append("}");
                    i++;
                }
                sb.Append("]");
                sb.Append("}");

                sb.Append("]"); // l : [
                sb.Append("}");
            } else { // Se l'oggetto NON ha delle pending policies allora posso servire l'oggetto stesso
                // Doing a IContentManager.Get here should ensure that the handlers that populate lazyfields
                // are executed, and all data in ContentParts and ContentField is initialized as it should.
                content = _orchardServices.ContentManager.Get(content.Id, VersionOptions.Published);

                dump = dumper.Dump(content, "Model");

                JsonConverter.ConvertToJSon(dump, sb, minified, realformat);
                sb.Insert(0, "{");
                sb.Append(", \"l\":[");
                // Dopo avere convertito il contentItem in JSON aggiungo i Json delle eventuali liste
                
                var dumperContext = new DumperServiceContext(
                    content,
                    () => 
                        new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour),
                    (_xElement, _stringBuilder) => 
                        JsonConverter.ConvertToJSon(_xElement, _stringBuilder, minified, realformat),
                    resultTarget,
                    page, pageSize);
                foreach (var dumperService in _dumperServices) {
                    dumperService.DumpList(dumperContext);
                }
                sb.Append(string.Join(",", dumperContext.ContentLists));

                sb.Append("]"); // l : [
                sb.Append("}");
            }
            jsonString = sb.ToString().Replace("\t", " ");
            result.Content = jsonString;
            return result;
        }

        private dynamic cleanobj(dynamic objec) {
            if (objec != null)
                if (objec.ToRemove != null) {
                    return cleanobj(objec.ToRemove);
                }
            return objec;
        }
    }
}