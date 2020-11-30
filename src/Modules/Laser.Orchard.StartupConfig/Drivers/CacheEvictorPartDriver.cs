using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartDriver : ContentPartCloningDriver<CacheEvictorPart> {
        private readonly IContentManager _contentManager;

        public CacheEvictorPartDriver(
            IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override void Importing(CacheEvictorPart part, ImportContentContext context) {
            // Replace idenfier with content id
            string ids = string.Empty;
            var evictItem = part.Settings.GetModel<CacheEvictorPartSettings>().EvictItem;
            if (!string.IsNullOrEmpty(evictItem)) {
                foreach (var item in evictItem.Split(';')) {
                    var content = context.GetItemFromSession(item);
                    if (content != null) {
                        ids += content.Id.ToString() + ";";
                    }
                }
                evictItem = ids;
            }

        }
        protected override void Exporting(CacheEvictorPart part, ExportContentContext context) {
            // Replace content id with id for identifier
            string identityId = string.Empty;
            var evictItem = part.Settings.GetModel<CacheEvictorPartSettings>().EvictItem;
            if (!string.IsNullOrEmpty(evictItem)) {
                int id;
                foreach (var item in evictItem.Split(';')) {
                    if (int.TryParse(item, out id)) {
                        var content = _contentManager.Get(id);
                        if (content.As<IdentityPart>() != null) {
                            identityId += content.As<IdentityPart>().Identifier+";";
                        }
                    }
                }
                part.Settings["CacheEvictorPartSettings.EvictItem"] = identityId;
            }
        }
    }
}