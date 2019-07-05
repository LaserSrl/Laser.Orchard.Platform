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

namespace Laser.Orchard.StartupConfig.ContentPickerContentCreation.Drivers {
    public class ContentPickerFieldCreateItemDriver : ContentFieldDriver<ContentPickerField> {
        private readonly IContentManager _contentManager;

        public ContentPickerFieldCreateItemDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentPickerField field, ContentPart part) {
            return field.Name;
        }


        protected override DriverResult Editor(ContentPart part, ContentPickerField field, dynamic shapeHelper) {

            var i = 0;

            return ContentShape("Fields_ContentPickerCreateItem_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new ContentPickerFieldViewModel {
                        Field = field,
                        Part = part,
                        ContentItems = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty).ToList()
                    };

                    model.SelectedIds = string.Join(",", field.Ids);

                    return shapeHelper.EditorTemplate(TemplateName: "Fields/ContentPickerCreateItem.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

    }
}