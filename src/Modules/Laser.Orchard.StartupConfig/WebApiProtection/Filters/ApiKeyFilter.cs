using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Filters;
using Orchard.Security;
using Orchard.Utility.Extensions;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json;
using System.Net;
using Laser.Orchard.StartupConfig.ViewModels;
using System.Net.Http;
using System.Net.Http.Formatting;
using Orchard.OutputCache;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Filters {

    /// <summary>
    /// A fini di test è possibile passare la ApiKey in QueryString nel seguente formato: OZVV5TpP4U6wJthaCORZEQ,10/03/2016T10.00.00+2
    /// Se ApiKey viene passato in QueryString non viene applicata la logica di cifratura.
    /// Se ApiKey viene passato in QueryString insieme al parametro clear=false invece, viene applicata la logica di cifratura.
    /// </summary>
    [OrchardFeature("Laser.Orchard.StartupConfig.WebApiProtection")]
    public class ApiKeyFilter : FilterProvider, IActionFilter, IResultFilter, ICachingEventHandler {
        private readonly IApiKeyService _apiKeyService;
        private readonly HttpRequest _request;
        private readonly IUtilsServices _utilsServices;
        private string _additionalCacheKey;

        public ApiKeyFilter(IApiKeyService apiKeyService, IUtilsServices utilsServices) {
            _request = HttpContext.Current.Request;
            Logger = NullLogger.Instance;
            _apiKeyService = apiKeyService;
            _utilsServices = utilsServices;
        }

        public ILogger Logger;

        private void ErrorResult(ActionExecutingContext filterContext, string errorData) {
            if (filterContext == null) return;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            var response = _utilsServices.GetResponse(ViewModels.ResponseType.UnAuthorized);
            response.Data = errorData;
            filterContext.Result = new JsonResult {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            return;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            _additionalCacheKey = _apiKeyService.ValidateRequestByApiKey(_additionalCacheKey);
            if ((_additionalCacheKey != null) && (_additionalCacheKey != "AuthorizedApi")) {
                ErrorResult(filterContext, String.Format("UnauthorizedApi: {0}", _request.QueryString["ApiKey"] ?? _request.Headers["ApiKey"]));
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }


        /// <summary>
        /// Called by OutpuCache after the default cache key has been defined
        /// </summary>
        /// <param name="key">default cache key such as defined in Orchard.OutpuCache</param>
        /// <returns>The new cache key</returns>
        public void KeyGenerated(StringBuilder key) {
            _additionalCacheKey = _apiKeyService.ValidateRequestByApiKey(_additionalCacheKey);
            key.Append(_additionalCacheKey);
        }
    }
}


