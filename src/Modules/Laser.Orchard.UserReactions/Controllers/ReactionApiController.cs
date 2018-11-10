using System;
using System.Web.Http;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.UserReactions.Controllers {
    
    [WebApiKeyFilter(false)]
    public class ReactionApiController : ApiController {
        private readonly IUserReactionsService _userReactionService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;

        public ReactionApiController(IUserReactionsService userReactionService, IOrchardServices orchardServices, IUtilsServices utilsServices) {
            _userReactionService = userReactionService;
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
        }

        public Response Get(int pageId) {
            if (pageId < 1) throw new Exception("Incorrect input parameter.");
            ReactionsSummaryVM typeClick = _userReactionService.GetSummaryReaction(pageId);
            return _utilsServices.GetResponse(ResponseType.Success, "", typeClick);
        }
        public Response Post(ReactionUpdateModel model) {
            ReactionsClickVM result = new ReactionsClickVM();
            if (model == null || model.TypeId < 1 || model.PageId < 1) {
                throw new Exception("Incorrect input parameter.");
            }

            result.Result = _userReactionService.CalculateTypeClick(model.TypeId, model.PageId);
            result.Status = _userReactionService.GetSummaryReaction(model.PageId);
            ResponseType rType = ResponseType.Success;
            if (result.Status.UserAuthorized == false) {
                rType = ResponseType.UnAuthorized;
            }
            return _utilsServices.GetResponse(rType, "", result);
        }
    }
    /// <summary>
    /// json example {"TypeId": 1, "PageId": 2817 }
    /// </summary>
    public class ReactionUpdateModel {
        public int TypeId { get; set; }
        public int PageId { get; set; }
    }
}