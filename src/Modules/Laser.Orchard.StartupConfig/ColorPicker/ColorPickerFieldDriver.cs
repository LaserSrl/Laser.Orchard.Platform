using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ColorPicker {
    public class ColorPickerFieldDriver : ContentFieldCloningDriver<ColorPickerField> {
        private const string TemplateName = "Fields/Custom.ColorPicker";
        public Localizer T { get; set; }
        public ColorPickerFieldDriver() {
            T = NullLocalizer.Instance;
        }
        private static string GetPrefix(ContentField field, ContentPart part) {
            return (part.PartDefinition.Name + "." + field.Name)
                   .Replace(" ", "_");
        }
        protected override DriverResult Display(ContentPart part, ColorPickerField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Custom_ColorPicker", // key in Shape Table
                field.Name, // used to differentiate shapes in Placement.info overrides, e.g. Fields_Common_Text-DIFFERENTIATOR
                            // this is the actual Shape which will be resolved
                            // (Fields/Custom.ColorPicker.cshtml)
                s =>
                s.Name(field.Name)
                    .Value(field.Value)
            );
        }
        protected override DriverResult Editor(ContentPart part, ColorPickerField field, dynamic shapeHelper) {
            return ContentShape("Fields_Custom_ColorPicker_Edit",
                field.Name,
                () => shapeHelper.EditorTemplate(
                          TemplateName: TemplateName,
                          Model: field,
                          Prefix: GetPrefix(field, part)));
        }
        protected override DriverResult Editor(ContentPart part, ColorPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field, shapeHelper);
        }
        protected override void Importing(ContentPart part, ColorPickerField field, ImportContentContext context) {
            var importedText = context.Attribute(GetPrefix(field, part), "Value");
            if (importedText != null) {
                field.Storage.Set(null, importedText);
            }
        }
        protected override void Exporting(ContentPart part, ColorPickerField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part))
                .SetAttributeValue("Value", field.Storage.Get<string>(null));
        }
        protected override void Cloning(ContentPart part, ColorPickerField originalField, ColorPickerField cloneField, CloneContentContext context) {
            cloneField.Value = originalField.Value;
        }
        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The Hex value string associated with the color (eg. #ff0000)."));
        }
    }
}