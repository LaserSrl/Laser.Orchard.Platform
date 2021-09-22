using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
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

            OnPublished<CacheEvictorPart>((context, part) => {
                if (part != null) {
                    // first part: evicting manually added ids
                    var evictItem = part.Settings.GetModel<CacheEvictorPartSettings>().EvictItem;
                    if (evictItem != null) {
                        var evictItems = evictItem
                            .Split(';')
                            // check only number
                            .Where(e => int.TryParse(e, out int num))
                            .ToList();

                        foreach (var item in evictItems) {
                            _cacheService.RemoveByTag(item);
                        }
                    }

                    var filterTermsRecordId = part.Settings.GetModel<CacheEvictorPartSettings>().FilterTermsRecordId;
                    if (filterTermsRecordId != null) {
                        // second part: term evict
                        // start with the selected ids
                        var allTermIds = filterTermsRecordId
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
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
                                .SelectMany(id => GetTerms(id, part.Settings.GetModel<CacheEvictorPartSettings>().IncludeChildren))
                                .Select(t => t.Id));
                            // if a taxonomy is selected get all its children
                            allIds.UnionWith(allTermIds
                               .Where(x => _taxonomyService.GetTaxonomy(x) != null)
                               .SelectMany(t => _taxonomyService.GetTerms(t))
                               .Select(term => term.Id));
                        }

                        // starting with the created list
                        // check the terms in my content
                        var termsPart = part.ContentItem
                            .As<TermsPart>();

                        var termParts = termsPart == null ? new List<TermPart>() :
                            termsPart
                            .TermParts
                            .Select(c => c.TermPart);

                        // and if present take the fathers to avoid them
                        // if parent ids are present in the list of selected terms in the settings, cache evict
                        foreach (var term in termParts) {
                            // check whether the end of the content item is present 
                            if (allIds.Contains(term.Id)) {
                                _cacheService.RemoveByTag(term.Id.ToString());
                            }
                            // checking if the taxonomy id is present
                            if (allIds.Contains(term.TaxonomyId)) {
                                _cacheService.RemoveByTag(term.Id.ToString());
                            }
                            // checking if the term hierarchy is present
                            foreach (var parent in _taxonomyService.GetParents(term).Where(p => allIds.Contains(p.Id))) {
                                _cacheService.RemoveByTag(parent.Id.ToString());
                            }
                        }
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