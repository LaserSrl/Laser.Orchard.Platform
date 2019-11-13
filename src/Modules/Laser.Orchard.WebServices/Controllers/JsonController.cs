using Contrib.Widgets.Services;
using Laser.Orchard.Commons.Services;
using Laser.Orchard.Events.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.WebServices.Models;
using Orchard;
using Orchard.Core.Contents;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization.Models;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Laser.Orchard.StartupConfig.Exceptions;
using Orchard.OutputCache;
using Laser.Orchard.Commons.Enums;
using Laser.Orchard.Policy.Services;

namespace Laser.Orchard.WebServices.Controllers
{

    public class JsonController : Controller, ICachingEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;



        private readonly ShellSettings _shellSetting;
        private readonly IUtilsServices _utilsServices;
        private IWidgetManager _widgetManager;
        private IEventsService _eventsService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPolicyServices _policyServices;

        private readonly HttpRequest _request;

        //
        // GET: /Json/
        public JsonController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService,
            ShellSettings shellSetting,
            IUtilsServices utilsServices,
            ICsrfTokenHelper csrfTokenHelper,
            IAuthenticationService authenticationService,
            IPolicyServices policyServices) {
            _request = System.Web.HttpContext.Current.Request;

            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
            _utilsServices = utilsServices;
            _csrfTokenHelper = csrfTokenHelper;
            _authenticationService = authenticationService;
            _policyServices = policyServices;
        }

        public ILogger Logger { get; set; }

        public ActionResult GetObjectById(int contentId = 0, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 1000, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, decimal version = 0) {
            if (contentId > 0) {
                IContent item = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
                if (item == null) {
                    Response rsp = _utilsServices.GetResponse(ResponseType.None, "Valore Id non valido");
                    // rsp.Message = ;
                    XmlSerializer serializer = new XmlSerializer(typeof(Response));
                    XmlDocument doc = new XmlDocument();
                    StringWriter sww = new StringWriter();
                    XmlWriter writer = XmlWriter.Create(sww);
                    serializer.Serialize(writer, rsp);
                    var xml = sww.ToString();
                    return this.Content(xml, "text/xml");
                } else {
                    if (item.As<AutoroutePart>() != null)
                        return GetObjectByAlias(item.As<AutoroutePart>().DisplayAlias, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse, deeplevel, version);
                    else {
                        Response rsp = _utilsServices.GetResponse(ResponseType.None);
                        rsp.Message = "Valore Id non valido";
                        XmlSerializer serializer = new XmlSerializer(typeof(Response));
                        XmlDocument doc = new XmlDocument();
                        StringWriter sww = new StringWriter();
                        XmlWriter writer = XmlWriter.Create(sww);
                        serializer.Serialize(writer, rsp);
                        var xml = sww.ToString();
                        return this.Content(xml, "text/xml");
                    }

                }
            } else {
                Response rsp = _utilsServices.GetResponse(ResponseType.None);
                rsp.Message = "Valore Id non valido";
                XmlSerializer serializer = new XmlSerializer(typeof(Response));
                XmlDocument doc = new XmlDocument();
                StringWriter sww = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sww);
                serializer.Serialize(writer, rsp);
                var xml = sww.ToString();
                return this.Content(xml, "text/xml");
            }
            //if (item == null) {
            //    new HttpException(404, ("Not found"));
            //    Response rsp = _utilsServices.GetResponse(ResponseType.None);
            //    rsp.Message = "Pagina non trovata";
            //    XmlSerializer serializer = new XmlSerializer(typeof(Response));
            //    XmlDocument doc = new XmlDocument();
            //    StringWriter sww = new StringWriter();
            //    XmlWriter writer = XmlWriter.Create(sww);
            //    serializer.Serialize(writer, rsp);
            //    var xml = sww.ToString();
            //    return this.Content(xml, "text/xml");
            //}
            //return (ContentResult)GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel);
        }

        public ActionResult GetObjectByAlias(string displayAlias, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 1000, bool tinyResponse = true, int deeplevel = 10, decimal version = 0) {
            try {

                if (page < 1)
                    page = 1;
                if (pageSize < 1)
                    pageSize = 1000;
                var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                    .ForVersion(VersionOptions.Published)
                    .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();
                IContent item = null;
                if (autoroutePart != null && autoroutePart.ContentItem != null) {
                    item = autoroutePart.ContentItem;
                } else {
                    Response rsp = _utilsServices.GetResponse(ResponseType.None, "Pagina non trovata");

                    XmlSerializer serializer = new XmlSerializer(typeof(Response));
                    XmlDocument doc = new XmlDocument();
                    StringWriter sww = new StringWriter();
                    XmlWriter writer = XmlWriter.Create(sww);
                    serializer.Serialize(writer, rsp);
                    var xml = sww.ToString();
                    return this.Content(xml, "text/xml");
                    // return new HttpNotFoundResult();
                }

                List<dynamic> ListShape = new List<dynamic>();
                string CiType = "";
                if (item.ContentItem.ContentType == "ProjectionPage") {

                    #region Projection

                    var part = ((dynamic)item.ContentItem).ProjectionPart;
                    if (part != null) {
                        var queryId = part.Record.QueryPartRecord.Id;

                        var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);

                        //   int i = 0;
                        if (queryItems.Count == 0) {
                            return null;
                        }
                        foreach (var projitem in queryItems) {
                            // autoroutePart = projitem.AutoroutePart;
                            CiType = projitem.ContentType;
                            List<dynamic> list2 = ItemsMultiLanguage(projitem.AutoroutePart);
                            //  var merged = new List<dynamic>(ListShape);
                            ListShape.AddRange(list2.Where(p2 =>
                           ListShape.All(p1 => p1.ContentItem.Id != p2.ContentItem.Id)));
                            // ListShape = merged;
                        }

                        #endregion Projection
                    }
                } else {
                    //  string tipoCI = item.ContentItem.ContentType;
                    //   string CiType = ((ContentItem)autoroutePart.ContentItem).ContentType;
                    //  int id = ((ContentItem)autoroutePart.ContentItem).Id;

                    ListShape = ItemsMultiLanguage(autoroutePart);
                    CiType = ((ContentItem)autoroutePart.ContentItem).ContentType;
                }

                //int masterid = 0;
                //int teoric_masterid = 0;
                //try {
                //    teoric_masterid = ((ContentItem)autoroutePart.ContentItem).As<LocalizationPart>().MasterContentItem.Id;
                //    masterid = teoric_masterid;
                //}
                //catch {
                //    masterid = id;
                //}
                //var contentsLocalized = _orchardServices.ContentManager.Query(CiType).Where<LocalizationPartRecord>(l => l.MasterContentItemId == masterid || l.Id == masterid).List();
                //List<dynamic> ListShape = new List<dynamic>();
                //foreach (ContentItem singleCi in contentsLocalized) {
                //    ListShape.Add(_orchardServices.ContentManager.BuildDisplay(singleCi));
                //}

                var namespaces = this.GetType().FullName.Split('.').AsEnumerable();
                namespaces = namespaces.Except(new string[] { this.GetType().Name });
                namespaces = namespaces.Except(new string[] { namespaces.Last() });
                var area = string.Join(".", namespaces);
                if (version > 0) {
                    CiType += version.ToString();
                }
                string myview = "~/" + @"App_Data/Sites/" + _shellSetting.Name + "/WebServices/" + CiType + ".cshtml";
                string myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\WebServices\" + CiType + ".cshtml";
                if (System.IO.File.Exists(myfile))
                    return View(myview, (object)ListShape);
                else {
                    Response rsp = _utilsServices.GetResponse(ResponseType.None, "Contenuto non disponibile");
                    //  rsp.Message =;
                    XmlSerializer serializer = new XmlSerializer(typeof(Response));
                    XmlDocument doc = new XmlDocument();
                    StringWriter sww = new StringWriter();
                    XmlWriter writer = XmlWriter.Create(sww);
                    serializer.Serialize(writer, rsp);
                    var xml = sww.ToString();
                    return this.Content(xml, "text/xml");
                }
            } catch (Exception ex) {
                Response rsp = _utilsServices.GetResponse(ResponseType.None, ex.Message);
                //   rsp.Message=;
                XmlSerializer serializer = new XmlSerializer(typeof(Response));
                XmlDocument doc = new XmlDocument();
                StringWriter sww = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sww);
                serializer.Serialize(writer, rsp);
                var xml = sww.ToString();
                return this.Content(xml, "text/xml");
            }
        }

        private List<dynamic> ItemsMultiLanguage(AutoroutePart autoroutePart) {
            string CiType = ((ContentItem)autoroutePart.ContentItem).ContentType;
            int id = ((ContentItem)autoroutePart.ContentItem).Id;
            //  string tipoCI = item.ContentItem.ContentType;
            //  int id = ((ContentItem)autoroutePart.ContentItem).Id;

            int masterid = 0;
            int teoric_masterid = 0;
            try {
                teoric_masterid = ((ContentItem)autoroutePart.ContentItem).As<LocalizationPart>().MasterContentItem.Id;
                masterid = teoric_masterid;
            } catch {
                masterid = id;
            }
            var contentsLocalized = _orchardServices.ContentManager.Query(CiType).Where<LocalizationPartRecord>(l => l.MasterContentItemId == masterid || l.Id == masterid).List();
            List<dynamic> ListShape = new List<dynamic>();
            foreach (ContentItem singleCi in contentsLocalized) {
                List<dynamic> list2 = new List<dynamic>();
                list2.Add(_orchardServices.ContentManager.BuildDisplay(singleCi));
                // var merged=new List<dynamic>(ListShape);
                ListShape.AddRange(list2.Where(p2 =>
                ListShape.All(p1 => p1.ContentItem.Id != p2.ContentItem.Id)));
                // ListShape = merged;
            }
            return ListShape;
        }

        //
        // GET: /Json/GetByAlias
        // Attributes:
        // displayAlias: url di ingresso Es: displayAlias=produttore-hermes
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        [AlwaysAccessible]
        public ActionResult GetByAlias(string displayAlias, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string complexBehaviour = "") {
            //   Logger.Error("inizio"+DateTime.Now.ToString());
            IContent item = null;

            if (displayAlias == null) {
                return Json(_utilsServices.GetResponse(ResponseType.MissingParameters), JsonRequestBehavior.AllowGet);
            }

            try {
                if (displayAlias.ToLower() == "user+info" || displayAlias.ToLower() == "user info") {
                    #region richiesta dati di uno user
                    var currentUser = _authenticationService.GetAuthenticatedUser();
                    if (currentUser == null) {
                        //  return Content((Json(_utilsServices.GetResponse(ResponseType.InvalidUser))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                        var result = new ContentResult { ContentType = "application/json" };
                        result.Content = Newtonsoft.Json.JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidUser));
                        return result;
                    } else
                        if (!_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                        var result = new ContentResult { ContentType = "application/json" };
                        result.Content = Newtonsoft.Json.JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidXSRF));
                        return result;
                        //   Content((Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                    } else {
                        #region utente validato
                        item = currentUser.ContentItem;

                        #endregion
                    }
                    #endregion
                } else {
                    item = GetContentByAlias(displayAlias);
                }

                if (!_orchardServices.Authorizer.Authorize(Permissions.ViewContent, item))
                    return Json(UnauthorizedResponse(), JsonRequestBehavior.AllowGet);

                if(item == null) {
                    return new HttpStatusCodeResult(404);
                }

                ContentResult cr = (ContentResult)GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, complexBehaviour.Split(','));
                //    Logger.Error("Fine:"+DateTime.Now.ToString());

                if (_orchardServices.WorkContext.CurrentSite.As<WebServiceSettingsPart>().LogWebservice) {
                    Logger.Error(cr.Content.ToString());
                }
                return cr;
            } catch (System.Security.SecurityException) {
                return Json(_utilsServices.GetResponse(ResponseType.InvalidUser), JsonRequestBehavior.AllowGet);
            } catch (OrchardSecurityException) {
                return Json(UnauthorizedResponse(), JsonRequestBehavior.AllowGet);
            } catch {
                return new HttpStatusCodeResult(500);
            }
        }

        private Response UnauthorizedResponse() {
            var response = _utilsServices.GetResponse(ResponseType.UnAuthorized);
            response.ResolutionAction = _authenticationService.GetAuthenticatedUser() == null ?
                ResolutionAction.Login :
                ResolutionAction.NoAction;
            return response;
        }

        //
        // GET: /Json/GetById
        // Attributes:
        // contentId: id del content Es: contentId=1
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        [Obsolete("Wil be removed for security problem")]
        public ContentResult GetById(int contentId, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string complexBehaviour = "") {
            IContent item = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
            if (item == null) {
                new HttpException(404, ("Not found"));
                return null;
            }
            return (ContentResult)GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, complexBehaviour.Split(','));
        }

        private ActionResult GetContent(IContent content, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string fieldspartsFilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string[] complexBehaviour = null) {
            var result = new ContentResult { ContentType = "application/json" };
            var jsonString = "{}";

            var _filterContentFieldsParts = fieldspartsFilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            XElement dump;
            XElement projectionDump = null;
            // il dump dell'oggetto principale non filtra per field
            ObjectDumper dumper = new ObjectDumper(deeplevel, null, false, tinyResponse, complexBehaviour);
            dynamic shape; //, specificShape;
            var sb = new StringBuilder();
            List<XElement> listContent = new List<XElement>();

            // verifico se l'oggetto è soggetto all'accettazione delle policies
            var policy = content.As<Policy.Models.PolicyPart>();
            //if (policy != null) {
            //    if ((String.IsNullOrWhiteSpace(_orchardServices.WorkContext.HttpContext.Request.QueryString["v"]))) {// E' soggetto a privacy, quindi faccio sempre il redirect se manca il parametro in querystring v=
            //        if (policy.HasPendingPolicies ?? false) { // se ha delle pending policies deve restituire le policy text, legate al contenuto, qui ndi non deve mai servire cache
            //            var redirectUrl = String.Format("{0}{1}v={2}", _orchardServices.WorkContext.HttpContext.Request.RawUrl, (_orchardServices.WorkContext.HttpContext.Request.RawUrl.Contains("?") ? "&" : "?"), Guid.NewGuid());
            //            _orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
            //        } else {// se NON ha delle pending policies deve restituire un url non cacheato (quindi aggiungo v=),
            //            var redirectUrl = String.Format("{0}{1}v={2}", _orchardServices.WorkContext.HttpContext.Request.RawUrl, (_orchardServices.WorkContext.HttpContext.Request.RawUrl.Contains("?") ? "&" : "?"), "cached-content");
            //            _orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
            //            //_orchardServices.WorkContext.HttpContext.Response.Redirect(redirectUrl, true);
            //        }
            //        return null; // in entrambi i casi ritorno null come risultato della current request
            //    }
            //}
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
                shape = _orchardServices.ContentManager.BuildDisplay(content);
                if (sourceType == SourceTypes.ContentItem) {
                    dump = dumper.Dump(content, "Model");
                } else {
                    dump = dumper.Dump(shape, "Model");
                }
                //dump.XPathSelectElements("");
                //var filteredDump = dump.Descendants();
                //ConvertToJSon(dump, sb);
                JsonConverter.ConvertToJSon(dump, sb, minified, realformat);
                sb.Insert(0, "{");
                sb.Append(", \"l\":[");
                // Dopo avere convertito il contentItem in JSON aggiungo i Json delle eventuali liste
                dynamic part = null;
                var firstList = true;
                var listDumpedContentIds = new List<int>();

                #region [ProjectionPart ]

                try {
                    part = shape.ContentItem.ProjectionPart;
                } catch {
                    part = null;
                }
                if (part != null) {
                    if (!firstList) {
                        sb.Append(",");
                    }
                    firstList = false;
                    var queryId = part.Record.QueryPartRecord.Id;
                    var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);
                    int i = 0;
                    sb.Append("{");
                    sb.AppendFormat("\"n\": \"{0}\"", "ProjectionList");
                    sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                    sb.Append(", \"m\": [");

                    foreach (var item in queryItems) {
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
                }
                part = null;

                #endregion [ProjectionPart ]

                #region [CalendarPart ]

                try {
                    part = shape.ContentItem.CalendarPart;
                } catch {
                    part = null;
                }
                if (part != null) {
                    if (!firstList) {
                        sb.Append(",");
                    }
                    firstList = false;
                    if (_orchardServices.WorkContext.TryResolve<IEventsService>(out _eventsService)) { // non sempre questo modulo è attivo quindi se non riesce a risolvere il servizio, bypassa la chiamata
                        var queryItems = _eventsService.GetAggregatedList(part, page, pageSize);
                        int i = 0;
                        sb.Append("{");
                        sb.AppendFormat("\"n\": \"{0}\"", "EventList");
                        sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                        sb.Append(", \"m\": [");

                        foreach (var item in queryItems) {
                            if (i > 0) {
                                sb.Append(",");
                            }
                            sb.Append("{");
                            dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour);
                            projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                            JsonConverter.ConvertToJSon(projectionDump, sb);
                            sb.Append("}");
                            i++;
                        }
                        sb.Append("]");
                        sb.Append("}");
                    }
                }
                part = null;

                #endregion [CalendarPart ]

                #region [ExernalField]

                var ExtertalFields = (dynamic)
                     (from parte in ((ContentItem)shape.ContentItem).Parts
                      from field in parte.Fields
                      where (field.GetType().Name == "FieldExternal" && ((dynamic)field).Setting.GenerateL)
                      select field).FirstOrDefault();
                if (ExtertalFields != null) {
                    if (!firstList) {
                        sb.Append(",");
                    }
                    firstList = false;
                    //sb.Append("{");
                    //sb.AppendFormat("\"n\": \"{0}\"", "ExternalContent");
                    //sb.AppendFormat(", \"v\": \"{0}\"", "ExternalContent");
                    //sb.Append(", \"m\": [");

                    sb.Append("{");
                    dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour);
                    //nameDynamicJsonArray = "List<generic>";
                    if (ExtertalFields.ContentObject != null) {
                        if (ExtertalFields.ContentObject.GetType() == typeof(ExternalFieldRemoteException)) {
                            throw new ExternalFieldRemoteException();
                        }
                        projectionDump = dumper.Dump(cleanobj(ExtertalFields.ContentObject), ExtertalFields.Name, "List<generic>");
                        JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                    }
                    //    sb.Append("}]}");
                    sb.Append("}");
                }

                #endregion [ExernalField]

                #region [ WidgetsContainerPart ]

                try {
                    part = shape.ContentItem.WidgetsContainerPart;
                } catch {
                    part = null;
                }
                if (part != null) {
                    //var queryId = part.Record.QueryPartRecord.Id;
                    if (_orchardServices.WorkContext.TryResolve<IWidgetManager>(out _widgetManager)) { // non semepre questo modulo è attivo quindi se non riesce a risolvere il servizio, bypassa la chiamata
                        if (!firstList) {
                            sb.Append(",");
                        }
                        firstList = false;
                        var queryItems = _widgetManager.GetWidgets(part.Id);
                        int i = 0;
                        sb.Append("{");
                        sb.AppendFormat("\"n\": \"{0}\"", "WidgetList");
                        sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                        sb.Append(", \"m\": [");

                        foreach (var item in queryItems) {
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
                    }
                }

                #endregion [ WidgetsContainerPart ]

                #region [ Taxonomy/TermsPart ]

                part = null;
                try {
                    if (shape.ContentItem.ContentType.EndsWith("Term") || !String.IsNullOrWhiteSpace(shape.ContentItem.TypeDefinition.Settings["Taxonomy"])) {
                        part = shape.ContentItem.TermPart;
                    }
                } catch {
                    part = null;
                }
                if (part != null) {
                    if (!firstList) {
                        sb.Append(",");
                    }
                    firstList = false;
                    dynamic termContentItems;
                    if (resultTarget == ResultTarget.Terms) {
                        termContentItems = _taxonomyService.GetChildren(part, true);
                    } else if (resultTarget == ResultTarget.SubTerms) {
                        termContentItems = _taxonomyService.GetChildren(part, false);
                    } else {
                        termContentItems = _taxonomyService.GetContentItems(part, (page - 1) * pageSize, pageSize);
                    }

                    int i = 0;
                    sb.Append("{");
                    if (resultTarget == ResultTarget.Contents) {
                        sb.AppendFormat("\"n\": \"{0}\"", "TaxonomyTermList");
                        sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                    } else {
                        sb.AppendFormat("\"n\": \"{0}\"", "TermPartList");
                        sb.AppendFormat(", \"v\": \"{0}\"", "TermPart[]");
                    }
                    sb.Append(", \"m\": [");

                    foreach (var item in termContentItems) {
                        if (i > 0) {
                            sb.Append(",");
                        }
                        sb.Append("{");
                        dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour);
                        if (resultTarget == ResultTarget.Contents) {
                            projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                            JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                        } else {
                            var dumperForPart = new ObjectDumper(deeplevel, _filterContentFieldsParts, true, tinyResponse, complexBehaviour);
                            projectionDump = dumperForPart.Dump(item, "TermPart");
                            JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                        }
                        sb.Append("}");
                        i++;
                    }
                    sb.Append("]");
                    sb.Append("}");
                }
                part = null;

                #endregion [ Taxonomy/TermsPart ]

                sb.Append("]"); // l : [
                sb.Append("}");
            }
            jsonString = sb.ToString().Replace("\t", " ");
            result.Content = jsonString;
            return result;
        }

        private dynamic cleanobj(dynamic objec) {
            if (objec != null)
                try {
                    if (objec.ToRemove != null) {
                        return cleanobj(objec.ToRemove);
                    }
                } catch { };
            return objec;
        }

        private IContent GetContentByAlias(string displayAlias) {
            IContent item = null;
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();

            if (autoroutePart != null && autoroutePart.ContentItem != null) {
                item = autoroutePart.ContentItem;
            } else {
                new HttpException(404, ("Not found"));
                return null;
            }
            return item;

        }
        /// <summary>
        /// Called by OutpuCache after the default cache key has been defined
        /// </summary>
        /// <param name="key">default cache key such as defined in Orchard.OutpuCache</param>
        /// <returns>The new cache key</returns>
        public void KeyGenerated(StringBuilder key) {
            var values = _request.RequestContext.RouteData.Values;
            if (values.ContainsKey("area") && values.ContainsKey("controller") && values.ContainsKey("action")) {
                if (values["area"].ToString().ToLowerInvariant().Equals("laser.orchard.webservices") &&
                    values["controller"].ToString().ToLowerInvariant().Equals("json") &&
                    values["action"].ToString().ToLowerInvariant().Equals("getbyalias")) {
                    IContent item = GetContentByAlias(_request.QueryString["displayalias"]);
                    if (item != null) {
                        var policy = item.As<Policy.Models.PolicyPart>();
                        if (policy != null && (_policyServices.HasPendingPolicies(item.ContentItem) ?? false)) {
                            key.Append("policy-not-accepted;");
                        } else if (policy != null && !(_policyServices.HasPendingPolicies(item.ContentItem) ?? false)) {
                            key.Append("policy-accepted;");
                        }
                    }
                }
            }
        }
    }
}