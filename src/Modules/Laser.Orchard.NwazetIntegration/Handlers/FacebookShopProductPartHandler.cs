using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.PartSettings;
using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Newtonsoft.Json.Linq;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Tokens;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    [OrchardFeature("Laser.Orchard.FacebookShop")]
    public class FacebookShopProductPartHandler : ContentHandler {
        IWorkContextAccessor _workContext;
        IFacebookShopService _facebookShopService;
        ITokenizer _tokenizer;

        public FacebookShopProductPartHandler(
            IRepository<FacebookShopProductPartRecord> repository,
            IWorkContextAccessor workContext,
            IFacebookShopService facebookShopService,
            ITokenizer tokenizer) {
            Filters.Add(StorageFilter.For(repository));
            _workContext = workContext;
            _facebookShopService = facebookShopService;
            _tokenizer = tokenizer;
        }

        protected override void Published(PublishContentContext context) {
            var product = context.ContentItem;

            try {
                var productPart = product.As<ProductPart>();
                var facebookPart = product.As<FacebookShopProductPart>();

                if (productPart != null && facebookPart != null && facebookPart.SynchronizeFacebookShop) {
                    var jsonTemplate = facebookPart.Settings.GetModel<FacebookShopProductPartSettings>().JsonForProductUpdate;
                        var fsssp = _workContext.GetContext().CurrentSite.As<FacebookShopSiteSettingsPart>();
                    if (string.IsNullOrWhiteSpace(jsonTemplate)) {
                        // Fallback to FacebookShopSiteSettingsPart
                        jsonTemplate = fsssp.DefaultJsonForProductUpdate;
                    }

                    if (!string.IsNullOrWhiteSpace(jsonTemplate)) {

                        // TODO: Newtonsoft Json to load the template into FacebookServiceJsonContext class.

                        string jsonBody = _tokenizer.Replace(jsonTemplate, context.ContentItem);
                        var url = "";
                    }
                }
            } catch {

            }

            base.Published(context);
        }
    }
}