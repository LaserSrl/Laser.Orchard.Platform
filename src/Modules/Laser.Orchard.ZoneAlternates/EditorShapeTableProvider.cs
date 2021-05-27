
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Html;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement.Descriptors;
using Orchard.UI.Admin;
using Orchard.DisplayManagement.Implementation;
using Orchard.Logging;

namespace Laser.Orchard.ZoneAlternates {
    public class EditorShapeTableProvider : IShapeTableProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public EditorShapeTableProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("EditorTemplate").OnDisplaying(context => {
                if (!AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext)) {
                    var alternateNameBase = "EditorTemplate";
                    var alternateName = alternateNameBase;
                    var alternateForPart = alternateNameBase;
                    var alternateForField = alternateNameBase;
                    var alternateForTypeField = alternateNameBase;
                    var alternateForType = alternateNameBase;
                    if (context.Shape.ContentItem != null) {
                        alternateName += "__" + context.Shape.ContentItem.ContentType;
                        alternateForType = alternateName;
                    }
                    if (context.Shape.ContentPart != null) {
                        alternateName += "__" + context.Shape.ContentPart.PartDefinition.Name;
                        alternateForPart = alternateNameBase + "__" + context.Shape.ContentPart.PartDefinition.Name;
                    }
                    if (context.Shape.ContentField != null) {
                        alternateName += "__" + context.Shape.ContentField.PartFieldDefinition.FieldDefinition.Name;
                        alternateForField = alternateNameBase + "__" + context.Shape.ContentField.PartFieldDefinition.FieldDefinition.Name;
                        alternateForTypeField = alternateForType + "__" + context.Shape.ContentField.PartFieldDefinition.FieldDefinition.Name;
                    }
                    if (!context.Shape.Metadata.Alternates.Contains(alternateName)) { // Fully qualified alternate EditorTemplate__User__UserPart__TextField
                        SetAlternateInHierarchy(alternateName, context);
                    }
                    if (!context.Shape.Metadata.Alternates.Contains(alternateForPart)) { // Alternate for generic Part EditorTemplate__UserPart
                        SetAlternateInHierarchy(alternateForPart, context);
                    }
                    if (context.Shape.ContentField != null) {
                        if (!context.Shape.Metadata.Alternates.Contains(alternateForField)) { // Alternate for generic Field EditorTemplate__TextField
                            SetAlternateInHierarchy(alternateForField, context, context.Shape.ContentField.Name);
                        }
                        if (!context.Shape.Metadata.Alternates.Contains(alternateForTypeField)) {// Alternate for Field in a content EditorTemplate__User__TextField
                            SetAlternateInHierarchy(alternateForTypeField, context, context.Shape.ContentField.Name);
                        }
                    }
                }
            });
        }
        private void SetAlternateInHierarchy(string alternateName, ShapeDisplayingContext context, string specializedName = null) {
            var progressiveHierarchyString = alternateName;
            var found = false;
            if (!context.Shape.Metadata.Alternates.Contains(alternateName)) {
                while (progressiveHierarchyString.Contains("__")) {
                    progressiveHierarchyString = progressiveHierarchyString.Substring(0, progressiveHierarchyString.LastIndexOf("__"));
                    if (context.Shape.Metadata.Alternates.Contains(progressiveHierarchyString)) {
                        context.ShapeMetadata.Alternates.Insert(context.Shape.Metadata.Alternates.IndexOf(progressiveHierarchyString) + 1, alternateName);
                        // Adding alternate name for shape template name.
                        context.ShapeMetadata.Alternates.Insert(context.Shape.Metadata.Alternates.IndexOf(progressiveHierarchyString) + 2, alternateName + "__" + EncodeAlternateElement(context.Shape.TemplateName));
                        found = true;
                        break;
                    }
                    else if (context.Shape.Metadata.Alternates.Contains(progressiveHierarchyString + "__" + specializedName)) { // if it's a field, the type of the field should be placed before the field name alternate
                        context.ShapeMetadata.Alternates.Insert(context.Shape.Metadata.Alternates.IndexOf(progressiveHierarchyString + "__" + specializedName), alternateName);
                        // Adding alternate name for shape template name.
                        context.ShapeMetadata.Alternates.Insert(context.Shape.Metadata.Alternates.IndexOf(progressiveHierarchyString + "__" + specializedName) + 1, alternateName + "__" + EncodeAlternateElement(context.Shape.TemplateName));
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    context.ShapeMetadata.Alternates.Add(alternateName);
                        // Adding alternate name for shape template name.
                    context.ShapeMetadata.Alternates.Add(alternateName + "__" + EncodeAlternateElement(context.Shape.TemplateName));
                }
            }
        }

        private string EncodeAlternateElement(string alternateElement) {
            // TODO: check if Replaces are ok.
            // After these Replaces, the string "__" is replaced with the string "-" in the list of Alternates.
            return alternateElement
                .Replace(".", "__")
                .Replace("\\", "__")
                .Replace("/", "__")
                .Replace("-", "__");
        }
    }
}