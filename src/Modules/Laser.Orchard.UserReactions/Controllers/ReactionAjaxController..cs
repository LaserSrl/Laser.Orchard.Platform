using System.Web;
using System.Web.Mvc;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using System;
using Orchard.Security;
using Orchard;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.UserReactions.Controllers {
    
    public class ReactionAjaxController : Controller     
    {
        private readonly IUserReactionsService _userReactionService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;

        public ReactionAjaxController(IUserReactionsService userReactionService, IOrchardServices orchardServices, IUtilsServices utilsServices) {
            _userReactionService = userReactionService;
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetReactionClicked(int reactionTypeId, int pageId) {
            ReactionsClickVM result = new ReactionsClickVM();
            var contentItem = _orchardServices.ContentManager.Get(pageId);
            if (_userReactionService.HasPermission(contentItem.ContentType) == false) {
                return Json(_utilsServices.GetResponse(ResponseType.UnAuthorized));
            }
            result.Result =_userReactionService.CalculateTypeClick(reactionTypeId, pageId);
            result.Status = _userReactionService.GetSummaryReaction(pageId);
            return Json(result) ;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSummaryReaction(int pageId) {
            ReactionsSummaryVM typeClick = _userReactionService.GetSummaryReaction(pageId);
            return Json(typeClick) ;
        }
    }
}