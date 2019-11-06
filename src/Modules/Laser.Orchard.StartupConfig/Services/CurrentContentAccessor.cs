using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System.Web.Routing;

namespace Laser.Orchard.StartupConfig.Services {
    public class CurrentContentAccessor : ICurrentContentAccessor {
        private readonly LazyField<ContentItem> _currentContentItemField = new LazyField<ContentItem>();
        private readonly IContentManager _contentManager;
        private readonly RequestContext _requestContext;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IWorkContextAccessor _workContext;

        public CurrentContentAccessor(
            IContentManager contentManager, 
            RequestContext requestContext,
            ICacheManager cacheManager,
            ISignals signals,
            IWorkContextAccessor wca) {

            _contentManager = contentManager;
            _requestContext = requestContext;
            _currentContentItemField.Loader(GetCurrentContentItem);
            _cacheManager = cacheManager;
            _signals = signals;
            _workContext = wca;
        }

        public ContentItem CurrentContentItem {
            get { return _currentContentItemField.Value; }
        }

        public int? CurrentContentItemId {
            get { return (GetCurrentContentItemId()); }
        }

        private string _keyBase = "";
        private string KeyBase {
            get {
                if (string.IsNullOrWhiteSpace(_keyBase)) {
                    var site = _workContext.GetContext()?.CurrentSite;
                    _keyBase = string.Join("_",
                        site?.BaseUrl ?? "",
                        site?.SiteName ?? "",
                        "Laser.Orchard.StartupConfig.Services.CurrentContentAccessor");
                }

                return _keyBase;
            }
        }
        private ContentItem GetCurrentContentItem() {
            var contentId = GetCurrentContentItemId();
            if (contentId == null) {
                return null;
            } else {
                var signalKey = $"CurrentContentAccessor_{contentId.Value}";
                var cacheKey = $"{KeyBase}_{signalKey}";
                return _cacheManager.Get(cacheKey, true, ctx => {
                    ctx.Monitor(_signals.When(signalKey));
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