using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Security;
using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartHandler : ContentHandler {
        private readonly IFacebookShopService _facebookShopService;
        private readonly INotifier _notifier;

        public FacebookShopProductPartHandler(
            IRepository<FacebookShopProductPartRecord> repository,
            IFacebookShopService facebookShopService,
            INotifier notifier) {
            Filters.Add(StorageFilter.For(repository));

            _facebookShopService = facebookShopService;
            _notifier = notifier;
            
            T = NullLocalizer.Instance;

            OnPublished<FacebookShopProductPart>((context, part) => AfterPublish(context));
            OnUnpublished<FacebookShopProductPart>((context, part) => AfterUnpublish(context));
            OnRemoved<FacebookShopProductPart>((context, part) => AfterRemove(context));
        }

        public Localizer T { get; set; }

        protected void AfterPublish(PublishContentContext context) {
            if (context.ContentItem.As<FacebookShopProductPart>().SynchronizeFacebookShop) {
                var result = _facebookShopService.SyncProduct(context.ContentItem);

                if (result == null || result.Requests.Count == 0) {
                    _notifier.Warning(T("Invalid Facebook api response. Product is not synchronised on Facebook catalog."));
                } else if (!result.Requests[0].Valid) {
                    _notifier.Warning(result.Requests[0].Message);
                }
            }
        }

        protected void AfterUnpublish(PublishContentContext context) {
            RemoveProduct(context.ContentItem);
        }

        protected void AfterRemove(RemoveContentContext context) {
            RemoveProduct(context.ContentItem);
        }

        private void RemoveProduct(ContentItem product) {
            if (product.As<FacebookShopProductPart>().SynchronizeFacebookShop) {
                var result = _facebookShopService.RemoveProduct(product);

                if (result == null || result.Requests.Count == 0) {
                    _notifier.Warning(T("Invalid Facebook api response. Product has not been removed from Facebook catalog."));
                } else if (!result.Requests[0].Valid) {
                    _notifier.Warning(result.Requests[0].Message);
                }
            }
        }
    }
}