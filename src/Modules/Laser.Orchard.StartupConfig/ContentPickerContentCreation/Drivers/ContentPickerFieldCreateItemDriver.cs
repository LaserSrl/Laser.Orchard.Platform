using System;
using System.Linq;
using Orchard.ContentPicker.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.ViewModels;
using Orchard.Localization;
using Orchard.Utility.Extensions;
using Orchard.ContentPicker.Fields;
using Laser.Orchard.StartupConfig.ContentPickerContentCreation.Settings;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents;
using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Laser.Orchard.StartupConfig.ContentPickerContentCreation.ViewModels;

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Drivers {
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

            if(!settings.EnableContentCreation){
                return null;
            }

            var CTs = GetCreatableTypes(false).ToList();
            List<String> contentTypeNames = new List<string>();
            foreach (var ct in CTs) {
                contentTypeNames.Add(ct.Name);
            }

            return ContentShape("Fields_ContentPickerCreateItem_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new ContentPickerCreateItemVM {
                        contentTypeList = contentTypeNames,
                        nameCPField = field.Name
                    };
                    return shapeHelper.EditorTemplate(TemplateName: "Fields/ContentPickerCreateItem.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

    }
}