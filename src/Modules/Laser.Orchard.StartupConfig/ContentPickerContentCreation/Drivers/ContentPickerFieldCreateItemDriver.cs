using Laser.Orchard.StartupConfig.ContentPickerContentCreation.Settings;
using Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentPicker.Fields;
using Orchard.ContentPicker.Settings;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Utility.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.ContentPickerContentCreation")]

    public class ContentPickerFieldCreateItemDriver : ContentFieldDriver<ContentPickerField> {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPickerFieldCreateItemDriver(IContentManager contentManager, IOrchardServices orchardServices, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            Services = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IOrchardServices Services { get; private set; }

        private static string GetPrefix(ContentPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentPickerField field, ContentPart part) {
            return field.Name;
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        protected override DriverResult Editor(ContentPart part, ContentPickerField field, dynamic shapeHelper) {

            var settings = field.PartFieldDefinition.Settings.GetModel<CPContentCreationSettings>();
            var fieldSettings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldSettings>();

            if (!settings.EnableContentCreation) {
                return null;
            }

            List<string> contentTypeNames = new List<string>();
            List<string> creatableTypes = GetCreatableTypes(false).Select(x => x.Name).ToList();

            if (fieldSettings != null && !string.IsNullOrWhiteSpace(fieldSettings.DisplayedContentTypes)) {
                var associatedContentTypes = fieldSettings.DisplayedContentTypes.Split(',');
                foreach (string contentTypeName in associatedContentTypes) {
                    if (!string.IsNullOrWhiteSpace(contentTypeName)) contentTypeNames.Add(contentTypeName.Trim());
                }
            } else {
                foreach (var creatableTypeName in creatableTypes) {
                    contentTypeNames.Add(creatableTypeName);
                }
            }

            return ContentShape("Fields_ContentPickerCreateItem_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new ContentPickerCreateItemVM {
                        contentTypeList = contentTypeNames,
                        nameCPField = field.Name,
                        multiple = fieldSettings.Multiple
                    };
                    return shapeHelper.EditorTemplate(TemplateName: "Fields/ContentPickerCreateItem.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, ContentPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, field, shapeHelper);
        }
    }
}