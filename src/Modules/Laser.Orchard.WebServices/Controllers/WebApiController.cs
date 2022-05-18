using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.WebServices.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.WebServices.Controllers {

    public class WebApiController : Controller, IWebApiService {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentSerializationServices _contentSerializationServices;


        private readonly ShellSettings _shellSetting;
        private readonly IUtilsServices _utilsServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IAuthenticationService _authenticationService;

        private readonly ICommonsServices _commonServices;

        private readonly HttpRequest _request;

        private List<string> processedItems;

        // GET: /Json/
        public WebApiController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService,
            ShellSettings shellSetting,
            IUtilsServices utilsServices,
            ICsrfTokenHelper csrfTokenHelper,
            IAuthenticationService authenticationService,
            ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices) {
            _request = System.Web.HttpContext.Current.Request;
            _commonServices = commonServices;
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
            _utilsServices = utilsServices;
            _csrfTokenHelper = csrfTokenHelper;
            _authenticationService = authenticationService;
            _contentSerializationServices = contentSerializationServices;
            processedItems = new List<string>();
        }

        public ILogger Logger { get; set; }

        [AlwaysAccessible]
        public ActionResult Terms(string alias, int maxLevel = 10) {
            var content = _commonServices.GetContentByAlias(alias);
            if (content == null) {
                return new HttpStatusCodeResult(404);
            }
            // TODO: Permissions
            var json = _contentSerializationServices.Terms(content, maxLevel);
            return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
        }

        [AlwaysAccessible]
        public ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10, string filter = "") {
            try {
                JObject json;

                if (alias == null) {
                    var result = new ContentResult { ContentType = "application/json" };
                    result.Content = JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.MissingParameters));
                    return result;
                }

                IContent content;
                if (alias.ToLower() == "user+info" || alias.ToLower() == "user info") {
                    #region [ Richiesta dati di uno user ]
                    var currentUser = _authenticationService.GetAuthenticatedUser();
                    if (currentUser == null) {
                        //  return Content((Json(_utilsServices.GetResponse(ResponseType.InvalidUser))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                        var result = new ContentResult { ContentType = "application/json" };
                        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        result.Content = JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidUser));
                        return result;
                    } else if (!_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                        var result = new ContentResult { ContentType = "application/json" };
                        Response.StatusCode = (int)HttpStatusCode.Unauthorized; // TODO: verify this is the correct code
                        result.Content = JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidXSRF));
                        return result;
                        //   Content((Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                    } else {
                        #region utente validato
                        content = currentUser.ContentItem;
                        #endregion
                    }
                    #endregion

                }
                else {
                    content = _commonServices.GetContentByAlias(alias);
                }

                if (content == null) {
                    return new HttpStatusCodeResult(404);
                }

                if (!_orchardServices.Authorizer.Authorize(Permissions.ViewContent, content)) {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Json(UnauthorizedResponse(), JsonRequestBehavior.AllowGet);
                }

                //_maxLevel = maxLevel;
                json = _contentSerializationServices.GetJson(content, page, pageSize, filter);
                //_contentSerializationServices.NormalizeSingleProperty(json);
                return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
                //return GetJson(content, page, pageSize);
            } catch (System.Security.SecurityException) {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(_utilsServices.GetResponse(ResponseType.InvalidUser), JsonRequestBehavior.AllowGet);
            } catch (Exception) {
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

    }

    public class EnumStringConverter : Newtonsoft.Json.Converters.StringEnumConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value.GetType().IsEnum) {
                writer.WriteValue(value.ToString());// or something else
                return;
            }
            base.WriteJson(writer, value, serializer);
        }
    }

    class CustomTermPart {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Slug { get; set; }
        public bool Selectable { get; set; }
    }
}