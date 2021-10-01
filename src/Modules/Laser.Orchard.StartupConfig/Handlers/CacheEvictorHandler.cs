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

            IEnumerable<int> previousTermIds = new List<int>();
            IEnumerable<int> previousTaxonomyIds = new List<int>();

            OnUpdating<CacheEvictorPart>((context, part) => {
                if (part != null && part.Settings.GetModel<CacheEvictorPartSettings>().EvictTerms) {
                    // saves the terms before saving
                    var termsPart = part.ContentItem
                        .As<TermsPart>();

                    if (termsPart != null) {
                        previousTaxonomyIds = termsPart.Terms.Select(t => t.TermRecord.TaxonomyId).ToList();
                        previousTermIds = termsPart.Terms.Select(t => t.TermRecord.Id).ToList();
                    }
                }
            });

            OnPublished<CacheEvictorPart>((context, part) => {
                if (part != null) {
                    HashSet<int> evictIds = new HashSet<int>();

                    // first part: evicting manually added ids
                    var evictItem = part.Settings.GetModel<CacheEvictorPartSettings>().EvictItem;
                    if (evictItem != null) {
                        var evictItems = evictItem
                            .Split(';')
                            // check only number
                            .Where(e => int.TryParse(e, out int num))
                            .Select(Int32.Parse)
                            .ToList();

                        evictIds.UnionWith(evictItems);
                    }
                    // second part: terms
                    if (part.Settings.GetModel<CacheEvictorPartSettings>().EvictTerms) {
                        // check the actually terms in my content.
                        var termsPart = part.ContentItem
                            .As<TermsPart>();

                        var taxonomyIds = termsPart.TermParts.Select(t => t.TermPart.TaxonomyId);
                        // add taxonomy ids of the previous taxonomy
                        evictIds.UnionWith(previousTaxonomyIds);
                        // add taxonomy ids of the terms
                        evictIds.UnionWith(taxonomyIds);

                        var terms = termsPart.TermParts.Select(c => c.TermPart);
                        // check if id of the term is present
                        foreach (var idTerm in previousTermIds.Except(terms.Select(t => t.Id))) {
                            // if previous id not is present in the list of actual
                            // finds term part for parents search 
                            evictIds.UnionWith(GetTermsToEvict(_taxonomyService.GetTerm(idTerm)));
                        }
                        // add term ids of the terms
                        foreach (var term in terms) {
                            evictIds.UnionWith(GetTermsToEvict(term));
                        }
                    }

                    // evict ids
                    foreach (var id in evictIds) {
                        _cacheService.RemoveByTag(id.ToString());
                    }
                }
            });
        }

        private List<int> GetTermsToEvict(TermPart termPart) {
            List<int> termIds = new List<int>();
            // evict term
            termIds.Add(termPart.Id);
            // evict parents
            foreach (var parent in _taxonomyService.GetParents(termPart)) {
                termIds.Add(parent.Id);
            }
            return termIds;
        }
    }
}