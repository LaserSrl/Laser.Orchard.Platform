using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement.Implementation;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Widgets.Models;


namespace Laser.Orchard.StartupConfig {


    public class AlternatesFactory : ShapeDisplayEvents {

        private string lastZone = "";
        private readonly IQueryService _queryService;
        private readonly ICurrentContentAccessor _currentContentAccessor;

        public AlternatesFactory(
            IQueryService queryService,
            ICurrentContentAccessor currentContentAccessor) {

            _queryService = queryService;
            _currentContentAccessor = currentContentAccessor;
        }
        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
              .OnDisplaying(displayedContext => {
                  if (displayedContext.ShapeMetadata.Type == "Zone") {
                      // we'll use this to have alternates for specific zones
                      lastZone = displayedContext.Shape.ZoneName;
                      // this adds alternates to personalize the entire zone for a specific
                      // ContentItem being displayed
                      var currentCI = _currentContentAccessor.CurrentContentItem;
                      if (currentCI != null) {
                          AddPersonalizedAlternates(displayedContext, currentCI, false);
                      }
                  } else if (displayedContext.Shape.ContentItem is ContentItem) {
                      // The item we are displaying
                      ContentItem contentItem = displayedContext.Shape.ContentItem;
                      // The part and field we are displaying, if any
                      ContentPart contentPart = displayedContext.Shape.ContentPart is ContentPart ? displayedContext.Shape.ContentPart : null;
                      ContentField contentField = displayedContext.Shape.ContentField is ContentField ? displayedContext.Shape.ContentField : null;
                      // Other elements we can use to personalize the alternate name
                      var displayType = displayedContext.ShapeMetadata.DisplayType;
                      var shapeName = displayedContext.ShapeMetadata.Type;
                      var zoneName = lastZone;
                      // delegate that will add the alternates to the list of usable ones
                      Action<string> AddAlternate = (s) => AddAlternateName(displayedContext.ShapeMetadata.Alternates, s);
                      // Add some alternates for ProjectionPart
                      if (contentPart.Is<ProjectionPart>() && contentPart.PartDefinition.Name == "ProjectionPart") {
                          var suffix = "";
                          if (displayedContext.ShapeMetadata.Type == "List" && displayedContext.Shape.PagerId != null) { // è una lista ma all'interno c'è il pager
                              suffix = "__Pager";
                              AddAlternate(shapeName + "__" + contentItem.ContentType + suffix);
                          }
                          if (contentPart.As<ProjectionPart>().Record.QueryPartRecord != null) {
                              var queryName = _queryService.GetQuery(contentPart.As<ProjectionPart>().Record.QueryPartRecord.Id).Name;
                              queryName = queryName.Normalize(System.Text.NormalizationForm.FormD).ToLower().Replace(" ", "");
                              AddAlternate(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName);
                              if (!String.IsNullOrWhiteSpace(zoneName)) {
                                  AddAlternate(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName + "__" + zoneName);
                              }
                          }
                      }
                      // The goal of the next few alternates is to enable very specific
                      // alternates for a given ContentItem, that are "portable", meaning that we can
                      // drop the cshtml on any environment we may have imported the content to and it
                      // will just work, without renaming or such. An alternative that gets close is
                      // to use and alternate based on the URL, but that suffers from cases where we 
                      // have tenants using the same theme (that contains the alternate) and with different
                      // url prefixes. 
                      // Using IdentityPart or AutoroutePart should allow us to get around that.
                      AddPersonalizedAlternates(displayedContext, contentItem);
                  } else {
                      // this adds alternates for other shapes in the page, that don't belong
                      // directly to the "main" content being displayed. For example, the menu 
                      // in the AsideSecond when a given content is displayed.
                      var currentCI = _currentContentAccessor.CurrentContentItem;
                      if (currentCI != null) {
                          AddPersonalizedAlternates(displayedContext, currentCI);
                      }
                  }

              });
        }
        private void AddPersonalizedAlternates(ShapeDisplayingContext context, ContentItem contentItem, bool considerZone = true) {

            var personalized = new List<string>();

            var identityPart = contentItem.As<IdentityPart>();
            if (identityPart != null) {
                var identifier = identityPart.Identifier;
                if (!string.IsNullOrWhiteSpace(identifier)) {
                    personalized.Add("__guid__" + identifier);
                }
            }
            var autoroutePart = contentItem.As<AutoroutePart>();
            if (autoroutePart != null) {
                var alias = autoroutePart.DisplayAlias;
                if (!string.IsNullOrWhiteSpace(alias)) {
                    personalized.Add("__alias__" + alias.Replace("-", "__").Replace(".", "_"));
                }
            }

            Func<string, bool> shouldVaryAlternate = (s) =>
                // avoid having both alias and guid in alternate name for the same content
                !personalized.Any(p => s.Contains(p))
                // avoid having the url portion for the alternate along with the others
                // (see Orchard.DesignerTools.Service.UrlAlternatesFactory)
                && !s.Contains("__url__");

            foreach (var alt in personalized) {
                AddCustomAlternates(context, alt, shouldVaryAlternate, considerZone);
            }
        }

        private void AddCustomAlternates(
            ShapeDisplayingContext context, string custom,
            Func<string, bool> shouldVary,
            bool considerZone = true) {
            if (!string.IsNullOrWhiteSpace(custom)) {
                // prevent adding these alternates again
                if (!context.ShapeMetadata.Alternates.Any(x => x.Contains(custom))) {
                    // delegate that will add the alternates to the list of usable ones
                    Action<string> AddAlternate = (s) => AddAlternateName(context.ShapeMetadata.Alternates, s);
                    // delegate to invoke adding the alternates
                    Action<string> AddPersonalized = (s) => AddAlternate(s);
                    // delegate that "multiplies" alternates by combining them with the identifier
                    Func<string, string[]> AlternateVariations = (baseName) => {
                        // some alternates should not be varied further
                        if (!shouldVary(baseName)) {
                            return new string[] { baseName };
                        }
                        return new string[] {
                            baseName,
                            baseName + custom
                        };
                    };
                    // Add alternates for the specific zone
                    if (considerZone && !string.IsNullOrWhiteSpace(lastZone)) {
                        AddPersonalized = (s) => {
                            AddAlternate(s);
                            AddAlternate(s + "__" + lastZone);
                        };
                        AlternateVariations = (baseName) => {
                            // some alternates should not be varied further
                            if (!shouldVary(baseName)) {
                                return new string[] { baseName };
                            }
                            return new string[] {
                                baseName,
                                baseName + custom,
                                baseName + custom + "__" + lastZone
                            };
                        };
                    }
                    // appends IdentityPart alternates to current ones
                    context.ShapeMetadata.Alternates = context.ShapeMetadata.Alternates
                      .SelectMany(
                          alternate => AlternateVariations(alternate)
                      ).ToList();
                    // appends [ShapeType]__[identifier] alternates
                    // appends [ShapeType]__[identifier]__[ZoneName] alternates
                    AddPersonalized(context.ShapeMetadata.Type + custom);
                }
            }
        }
        
        private static void AddAlternateName(IList<string> alternateNames, string name) {
            if (!alternateNames.Contains(name)) {
                alternateNames.Add(name);
            }
        }
    }
}