using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartHandler : ContentHandler {
        IFacebookShopService _facebookShopService;
        INotifier _notifier;

        public FacebookShopProductPartHandler(
            IRepository<FacebookShopProductPartRecord> repository,
            IFacebookShopService facebookShopService,
            INotifier notifier) {

            Filters.Add(StorageFilter.For(repository));
            _facebookShopService = facebookShopService;
            _notifier = notifier;

            T = NullLocalizer.Instance;

            OnPublished<FacebookShopProductPart>((context, part) => AfterPublish(context));
        }

        public Localizer T { get; set; }

        protected void AfterPublish(PublishContentContext context) {
            var result = _facebookShopService.SyncProduct(context.ContentItem);

            if (result != null && result.Valid) {

            } else {
                _notifier.Warning(result.Message);
            }
        }
    }
}