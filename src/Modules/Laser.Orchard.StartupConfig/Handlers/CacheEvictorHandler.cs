using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorHandler : ContentHandler {
        private readonly ICacheService _cacheService;
        private readonly ITaxonomyService _taxonomyService;


        public CacheEvictorHandler(
            ICacheService cacheService,
            ITaxonomyService taxonomyService) {
            _cacheService = cacheService;
            _taxonomyService = taxonomyService;

            OnPublished<IContent>((context, part) => {
                var cacheEvictorPart = part.As<CacheEvictorPart>();
                if (cacheEvictorPart != null) {
                    var allTermIds = cacheEvictorPart.Settings
                        .GetModel<CacheEvictorPartSettings>().FilterTermsRecordId
                        .Split(';')
                        .Select(int.Parse)
                        .ToList();

                    // create a single list with all the ids to be evicted
                    // used the hashset because it manages duplicate ids by itself
                    HashSet<int> allIds = new HashSet<int>();
                    allIds.UnionWith(allTermIds);
                    if (allTermIds.Any()) {
                        // for all ids that are terms
                        // depending on the setting get or don't get the children 
                        allIds.UnionWith(allTermIds
                            .Where(id => _taxonomyService.GetTaxonomy(id) == null)
                            .SelectMany(id => GetTerms(id, cacheEvictorPart.Settings.GetModel<CacheEvictorPartSettings>().IncludeChildren))
                            .Select(t => t.Id));
                        // if a taxonomy is selected get all its children
                        allIds.UnionWith(allTermIds
                           .Where(x => _taxonomyService.GetTaxonomy(x) != null)
                           .SelectMany(t => _taxonomyService.GetTerms(t))
                           .Select(term => term.Id));
                    }

                    // add item id 
                    allIds.UnionWith(
                        cacheEvictorPart.Settings
                        .GetModel<CacheEvictorPartSettings>().EvictItem
                        .Split(';')
                        // check only number
                        .Where(e => int.TryParse(e, out int num))
                        .Select(int.Parse)
                        .ToList());

                    foreach (var item in allIds) {
                        _cacheService.RemoveByTag(item.ToString());
                    }
                }
            });
        }

        private IEnumerable<TermPart> GetTerms(int termId, bool andChildren) {
            var term = _taxonomyService.GetTerm(termId);
            if (term == null) {
                return Enumerable.Empty<TermPart>();
            }
            var ts = new List<TermPart>() { term };
            if (andChildren) {
                ts.AddRange(_taxonomyService.GetChildren(term));
            }
            return ts;
        }
    }
}