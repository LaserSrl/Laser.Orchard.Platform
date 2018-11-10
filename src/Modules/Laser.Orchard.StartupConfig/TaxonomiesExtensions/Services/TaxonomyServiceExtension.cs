using System.Collections.Generic;
using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Extensions;
using Orchard.Environment.State;
using Orchard.Security;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyServiceExtension : TaxonomyServiceDraftable, ITaxonomyService {
        private readonly TaxonomyService _taxonomyService;

        
        public TaxonomyServiceExtension(IRepository<TermContentItem> termContentItemRepository, IContentManager contentManager, INotifier notifier, IContentDefinitionManager contentDefinitionManager, IAuthorizationService authorizationService, IOrchardServices services, IProcessingEngine processingEngine, ShellSettings shellSettings, IShellDescriptorManager shellDescriptorManager, TaxonomyService taxonomyService) : base(termContentItemRepository, contentManager, notifier, contentDefinitionManager, authorizationService, services, processingEngine, shellSettings, shellDescriptorManager) {
            _taxonomyService = taxonomyService;
        }

        public new IEnumerable<IContent> GetContentItems(TermPart term, int skip = 0, int count = 0, string fieldName = null) {
            var taxonomy = _taxonomyService.GetTaxonomy(term.TaxonomyId);
            var baseQuery = GetContentItemsQuery(term, fieldName);
            IEnumerable<IContent> termContentItems;

            if (taxonomy.ContentItem.As<TaxonomyExtensionPart>().OrderBy == OrderType.PublishedUtc) {
                termContentItems = baseQuery
                                    .Join<CommonPartRecord>()
                                    .OrderByDescending(x => x.PublishedUtc)
                                    .Slice(skip, count);
            }
            else if (taxonomy.ContentItem.As<TaxonomyExtensionPart>().OrderBy == OrderType.Title) {
                termContentItems = baseQuery
                                    .Join<TitlePartRecord>()
                                    .OrderBy(x => x.Title)
                                .Slice(skip, count);
            }
            else {
                termContentItems = baseQuery
                .Join<CommonPartRecord>()
                .OrderByDescending(x => x.CreatedUtc)
                .Slice(skip, count);

            }
            return termContentItems;
        }

    }
}