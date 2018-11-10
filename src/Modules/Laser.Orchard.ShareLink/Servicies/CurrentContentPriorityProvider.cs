using Laser.Orchard.ShareLink.Models;
using System.Web.Routing;

namespace Laser.Orchard.ShareLink.Servicies {
    public class CurrentContentPriorityProvider : IShareLinkPriorityProvider {

        private readonly RequestContext _requestContext;

        public CurrentContentPriorityProvider(
            RequestContext requestContext) {

            _requestContext = requestContext;
        }

        public int GetPriority(ShareLinkPart part) {
            var currentId = GetCurrentContentItemId();
            return currentId.HasValue && currentId.Value == part.ContentItem.Id ? 10 : 0;
        }

        private int? GetCurrentContentItemId() {
            object id;
            if (_requestContext.RouteData.Values.TryGetValue("id", out id)) {
                int contentId;
                if (int.TryParse(id as string, out contentId))
                    return contentId;
            }
            return null;
        }

    }
}