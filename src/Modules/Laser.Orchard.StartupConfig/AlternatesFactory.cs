using System;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Widgets.Models;


namespace Laser.Orchard.StartupConfig {


    public class AlternatesFactory : ShapeDisplayEvents {

        private string lastZone = "";
        private readonly IQueryService _queryService;
        public AlternatesFactory(IQueryService queryService) {
            _queryService = queryService;
        }
        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
              .OnDisplaying(displayedContext => {
                  if (displayedContext.ShapeMetadata.Type == "Zone") {
                      lastZone = displayedContext.Shape.ZoneName;
                  } else if (displayedContext.Shape.ContentItem is ContentItem) {
                      ContentItem contentItem = displayedContext.Shape.ContentItem;
                      ContentPart contentPart = displayedContext.Shape.ContentPart is ContentPart ? displayedContext.Shape.ContentPart : null;
                      ContentField contentField = displayedContext.Shape.ContentField is ContentField ? displayedContext.Shape.ContentField : null;
                      var displayType = displayedContext.ShapeMetadata.DisplayType;
                      var shapeName = displayedContext.ShapeMetadata.Type;
                      var zoneName = lastZone;
                      if (contentPart.Is<ProjectionPart>() && contentPart.PartDefinition.Name == "ProjectionPart") {
                          var suffix = "";
                          if (displayedContext.ShapeMetadata.Type == "List" && displayedContext.Shape.PagerId != null) { // è una lista ma all'interno c'è il pager
                              suffix = "__Pager";
                              if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + suffix)) {
                                  displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + suffix);
                              }
                          }
                          if (contentPart.As<ProjectionPart>().Record.QueryPartRecord != null) {
                              var queryName = _queryService.GetQuery(contentPart.As<ProjectionPart>().Record.QueryPartRecord.Id).Name;
                              queryName = queryName.Normalize(System.Text.NormalizationForm.FormD).ToLower().Replace(" ", "");
                              if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName)) {
                                  displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName);
                              }
                              if (!String.IsNullOrWhiteSpace(zoneName)) {
                                  if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName + "__" + zoneName)) {
                                      displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + suffix + "__ForQuery__" + queryName + "__" + zoneName);
                                  }
                              }
                          }
                      }
                  }
              });
        }
    }
}