using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using System.Linq;
using System.Xml.Linq;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorHandler : ContentHandler {
        private readonly ICacheService _cacheService;

        public CacheEvictorHandler(
            ICacheService cacheService) {
            _cacheService = cacheService;

            OnPublished<IContent>((context, part) => {
                //cycle through all parts
                foreach (var pa in part.ContentItem.Parts) {
                    var setting = pa.Settings.GetModel<CacheEvictorPartSettings>().EvictItem;
                    if (!string.IsNullOrEmpty(setting)) {
                        int id;
                        foreach (var item in setting.Split(';')) {
                            if (int.TryParse(item, out id)) {
                                _cacheService.RemoveByTag(item);
                            }
                        }
                    }
                }
            });
        }
    }
}