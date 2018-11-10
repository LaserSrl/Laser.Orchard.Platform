using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Settings {
    public abstract class GDPRPartFieldEditorEventsBase : ContentDefinitionEditorEventsBase {

        protected readonly IContentDefinitionManager _contentDefinitionManager;

        public GDPRPartFieldEditorEventsBase(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        // The handler methods for fields do not have knowledge on the type definition, so 
        // from just the information they have in their parameter it would be impossible to 
        // know whether the type has a GDPRPart. However, this handler is instanced only once
        // per request, so I can verify the presence of a GDPRPart in the method that takes
        // care of the parts, and store that information in a private field.
        protected bool _typeHasGDPRPart { get; set; }

        /// <summary>
        /// This is used in the DefinitionTemplates to know the ids of the divs where the settings
        /// related to GDPR are.
        /// </summary>
        protected string PartName { get; set; }

        private string BaseDivId(string fieldName) {
            return $"{PartName}_{fieldName}";
        }

        protected string AnonymizationDivId(string fieldName) {
            return $"{BaseDivId(fieldName)}_Anonymization";
        }

        protected string ErasureDivId(string fieldName) {
            return $"{BaseDivId(fieldName)}_Erasure";
        }

        // Use the part methods to check whether the type has a GDPRPart. The way we are using this bool value,
        // we have these settings also on ContentFields that may be attached to the GDPRPart itself.
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            _typeHasGDPRPart |=
                definition
                .ContentTypeDefinition
                .Parts
                .Any(ctpd =>
                    ctpd.PartDefinition.Name == "GDPRPart");

            PartName = definition.PartDefinition.Name;

            yield break;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            _typeHasGDPRPart |=
                _contentDefinitionManager
                .GetTypeDefinition(builder.TypeName)
                .Parts
                .Any(ctpd =>
                    ctpd.PartDefinition.Name == "GDPRPart");

            PartName = builder.Name;

            yield break;
        }

    }
}