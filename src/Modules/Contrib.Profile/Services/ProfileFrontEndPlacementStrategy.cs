using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Services;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Profile.Services {
    public class ProfileFrontEndPlacementStrategy : IShapeTableEventHandler {

        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;
        private readonly IFrontEndProfileService _frontEndProfileService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ProfileFrontEndPlacementStrategy(
            Work<IContentDefinitionManager> contentDefinitionManager,
            IFrontEndProfileService frontEndProfileService,
            IWorkContextAccessor workContextAccessor) {

            _contentDefinitionManager = contentDefinitionManager;
            _frontEndProfileService = frontEndProfileService;
            _workContextAccessor = workContextAccessor;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ShapeTableCreated(ShapeTable shapeTable) {

            var typeDefinitions = _contentDefinitionManager.Value
                .ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart"));

            var allPlacements = typeDefinitions
                .SelectMany(td => _frontEndProfileService.GetFrontEndPlacement(td)
                    .Select(p => new TypePlacement { Placement = p, ContentType = td.Name }));

            // group all placement settings by shape type
            var shapePlacements = allPlacements
                .GroupBy(x => x.Placement.ShapeType)
                .ToDictionary(x => x.Key, y => y.ToList());

            foreach (var shapeType in shapeTable.Descriptors.Keys) {
                List<TypePlacement> customPlacements;
                if (shapePlacements.TryGetValue(shapeType, out customPlacements)) {
                    if (!customPlacements.Any()) {
                        continue;
                    }
                    // there are some custom placements, build a predicate
                    var descriptor = shapeTable.Descriptors[shapeType];
                    var placement = descriptor.Placement;
                    descriptor.Placement = ctx => {
                        var WorkContext = _workContextAccessor.GetContext(); //I need the context for the call using the predicates
                        if (ctx.DisplayType == null &&
                            !AdminFilter.IsApplied(WorkContext.HttpContext.Request.RequestContext)) {

                            foreach (var customPlacement in customPlacements) {
                                var type = customPlacement.ContentType;
                                var differentiator = customPlacement.Placement.Differentiator;

                                if (((ctx.Differentiator ?? string.Empty) == (differentiator ?? string.Empty)) && ctx.ContentType == type) {

                                    var location = customPlacement.Placement.Zone;
                                    if (!string.IsNullOrEmpty(customPlacement.Placement.Position)) {
                                        location = string.Concat(location, ":", customPlacement.Placement.Position);
                                    }

                                    return new PlacementInfo { Location = location };
                                }
                            }
                        }
                        //fallback
                        return placement(ctx);
                    };
                }
            }

        }
        
    }
}