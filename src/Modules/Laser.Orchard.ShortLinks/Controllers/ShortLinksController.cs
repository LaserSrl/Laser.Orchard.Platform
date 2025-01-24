using Laser.Orchard.ShortLinks.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Security;
using System.Web.Mvc;

namespace Laser.Orchard.ShortLinks.Controllers {
    public class ShortLinksController : Controller {
        private readonly IShortLinksService _shortLinksService;
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public ShortLinksController(IShortLinksService shortLinksService,
            IAuthorizer authorizer,
            IContentManager contentManager,
            IOrchardServices orchardServices) {

            _shortLinksService = shortLinksService;
            _authorizer = authorizer;
            _contentManager = contentManager;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        [Authorize]
        public JsonResult GenerateShortLink(int contentId) {
            var content = _contentManager.Get(contentId);

            if (content == null) {
                return ErrorJson(T("Invalid content."));
            }

            if (!_authorizer.Authorize(Permissions.EditContent, content)) {
                return ErrorJson(T("Unauthorized."));
            }

            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var contentUrl = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(content));
            var shortUrl = _shortLinksService.GetShortLink(contentUrl);

            if (!string.IsNullOrWhiteSpace(shortUrl)) {
                return SuccessJson(shortUrl);
            }

            return ErrorJson(T("Failed to create a short url for the content."));
        }

        private JsonResult ErrorJson(LocalizedString message) {
            return Json(new { ko = "ko", message = message.Text });
        }

        private JsonResult SuccessJson(string result) {
            return Json(new { ok = "ok", shorturl = result });
        }
    }
}