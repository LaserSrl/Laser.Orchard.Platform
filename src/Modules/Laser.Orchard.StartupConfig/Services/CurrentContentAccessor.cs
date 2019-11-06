using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.StartupConfig.Services {
    public class CurrentContentAccessor : ICurrentContentAccessor {
        private readonly LazyField<ContentItem> _currentContentItemField = new LazyField<ContentItem>();
        private readonly IContentManager _contentManager;
        private readonly RequestContext _requestContext;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public CurrentContentAccessor(
            IContentManager contentManager, 
            RequestContext requestContext,
            ICacheManager cacheManager,
            ISignals signals) {

            _contentManager = contentManager;
            _requestContext = requestContext;
            _currentContentItemField.Loader(GetCurrentContentItem);
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public ContentItem CurrentContentItem {
            get { return _currentContentItemField.Value; }
        }

        public int? CurrentContentItemId {
            get { return (GetCurrentContentItemId()); }
        }
        private ContentItem GetCurrentContentItem() {
            var contentId = GetCurrentContentItemId();
            if (contentId == null) {
                return null;
            } else {
                var key = $"CurrentContentAccessor_{contentId.Value}";
                return _cacheManager.Get(key, true, ctx => {
                    ctx.Monitor(_signals.When(key));
                    return _contentManager.Get(contentId.Value);
                });
            }
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