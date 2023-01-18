using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System.Collections.Generic;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.Services {
    public class CurrentContentAccessor : ICurrentContentAccessor {
        private readonly LazyField<ContentItem> _currentContentItemField = new LazyField<ContentItem>();
        private readonly IContentManager _contentManager;
        private readonly RequestContext _requestContext;

        public CurrentContentAccessor(
            IContentManager contentManager, 
            RequestContext requestContext) {

            _contentManager = contentManager;
            _requestContext = requestContext;

            _currentContentItemField.Loader(GetCurrentContentItem);

            _contentItemMemory = new Dictionary<int, ContentItem>();
        }

        public ContentItem CurrentContentItem {
            get { return _currentContentItemField.Value; }
        }

        public int? CurrentContentItemId {
            get { return (GetCurrentContentItemId()); }
        }

        private Dictionary<int, ContentItem> _contentItemMemory;
        private ContentItem GetCurrentContentItem() {
            var contentId = GetCurrentContentItemId();
            if (contentId == null) {
                return null;
            } else {
                ContentItem ci = null;
                if (!_contentItemMemory.ContainsKey(contentId.Value)) {
                    try {
                        _contentItemMemory.Add(contentId.Value, _contentManager.Get(contentId.Value));
                    } catch { }
                }
                ci = _contentItemMemory[contentId.Value];
                // rehydrate ContentManager to prevent expired lifetime scopes
                if (ci != null) {
                    ci.ContentManager = _contentManager;
                }
                return ci;
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