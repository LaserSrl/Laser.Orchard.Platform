using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.UserProfiler.Service;
using Orchard;
using System.Collections.Generic;
using System.Web.Http;

namespace Laser.Orchard.UserProfiler.Controllers {

    [WebApiKeyFilter(true)]
    public class ProfileApiController : ApiController {
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUserProfilingService _userProfilingService;

        public ProfileApiController(
            IUtilsServices utilsServices,
            IOrchardServices orchardServices,
            ICsrfTokenHelper csrfTokenHelper,
            IUserProfilingService userProfilingService) {
            _utilsServices = utilsServices;
            _orchardServices = orchardServices;
            _csrfTokenHelper = csrfTokenHelper;
            _userProfilingService = userProfilingService;
        }

        public class parameterint {
            public int id { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pint">
        /// {"id":15}</param>
        /// <returns></returns>
        [HttpPost]
        public Response PostId(parameterint pint) {
            bool saltotest = false;
            if (pint == null)
                return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else {
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken() || saltotest) {
                    if (pint.id > 0)
                        return post_method(currentUser.Id, pint.id);
                    else
                        return _utilsServices.GetResponse(ResponseType.Validation, "Content not found");
                }
                else return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
            }
        }

        public class listparameterint {
            public List<int> id { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="listIds">
        /// {"id":[15,2]}</param>
        /// <returns></returns>
        [HttpPost]
        public Response PostIds(listparameterint listIds) {
            bool saltotest = false;
            if (listIds == null || listIds.id == null)
                return _utilsServices.GetResponse(ResponseType.Validation, "Unknow Parameter type");
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser == null)
                return _utilsServices.GetResponse(ResponseType.UnAuthorized);
            else {
                if (_csrfTokenHelper.DoesCsrfTokenMatchAuthToken() || saltotest) {
                    return post_method(currentUser.Id, listIds.id);
                }
                else return (_utilsServices.GetResponse(ResponseType.InvalidXSRF));// { Message = "Invalid Token/csrfToken", Success = false, ErrorCode=ErrorCode.InvalidXSRF,ResolutionAction=ResolutionAction.Login });
            }
        }

        #region [ post_method]

        private Response post_method(int userid, int id) {
            return _utilsServices.GetResponse(ResponseType.Success, "", _userProfilingService.UpdateUserProfile(userid, id));
        }

        private Response post_method(int userid, List<int> ids) {
            //var totaloutput = new Dictionary<string, int>();
            //foreach (var id in ids) {
            //    var partial = _userProfilingService.UpdateUserProfile(userid, id);
            //    foreach (var key in partial.Keys) {
            //        if (totaloutput.ContainsKey(key))
            //            totaloutput[key] = partial[key];
            //        else
            //            totaloutput.Add(key, partial[key]);
            //    }
            //}
            var totaloutput = _userProfilingService.UpdateUserProfile(userid, ids);
            return _utilsServices.GetResponse(ResponseType.Success, "", totaloutput);
        }

        #endregion [ post_method]
    }
}