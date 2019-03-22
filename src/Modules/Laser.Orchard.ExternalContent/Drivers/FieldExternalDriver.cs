using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.ExternalContent.Services;
using Laser.Orchard.ExternalContent.Settings;
using Laser.Orchard.ExternalContent.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Utility.Extensions;
using System.Web.Hosting;

namespace Laser.Orchard.ExternalContent.Drivers {
    public class FieldExternalDriver : ContentFieldCloningDriver<FieldExternal> {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly IFieldExternalService _fieldExternalService;
        public Localizer T { get; set; }

        public FieldExternalDriver(ShellSettings shellSettings, IOrchardServices orchardServices, IFieldExternalService fieldExternalService
            ) {
                _fieldExternalService = fieldExternalService;
                _orchardServices = orchardServices;
                _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
            string mobile_folder = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Xslt\";
            if (!System.IO.Directory.Exists(mobile_folder))
                System.IO.Directory.CreateDirectory(mobile_folder);
        }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, FieldExternal field, string displayType, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();
            return ContentShape("Fields_FieldExternal", GetDifferentiator(field, part), () => shapeHelper.Fields_FieldExternal(ContentPart: part, ContentField: field, Setting: settings));
        }

        protected override DriverResult Editor(ContentPart part, FieldExternal field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();

            var viewModel = new FieldExternalVM {
                Setting = settings,
                Field = field
            };

            return ContentShape("Fields_FieldExternal_Edit", GetDifferentiator(field, part),
                                () => shapeHelper.EditorTemplate(TemplateName: "Fields/FieldExternal", Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, FieldExternal field, IUpdateModel updater, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();
            var viewModel = new FieldExternalVM {
                Setting = settings,
                Field = field
            };

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {

            }
            if (settings.Required && string.IsNullOrEmpty(settings.ExternalURL) && string.IsNullOrEmpty(field.ExternalUrl)) {
                updater.AddModelError("External Url", T("The field {0} is mandatory", field.Name.CamelFriendly()));
            }
            if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.StartSchedule") {
                _fieldExternalService.ScheduleNextTask(settings.ScheduledMinute, part.ContentItem);
            }
                     
            
            return Editor(part, field, shapeHelper);
        }
        protected override void Exporting(ContentPart part, FieldExternal field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("BodyRequest", field.BodyRequest);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("ExternalUrl", field.ExternalUrl);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("HttpDataTypeCode", field.HttpDataTypeCode);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("HttpVerbCode", field.HttpVerbCode);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("AdditionalHeadersText", field.AdditionalHeadersText);
        }
        protected override void Cloning(ContentPart part, FieldExternal originalField, FieldExternal cloneField, CloneContentContext context) {
            cloneField.ExternalUrl = originalField.ExternalUrl;
            cloneField.HttpVerbCode = originalField.HttpVerbCode;
            cloneField.HttpDataTypeCode = originalField.HttpDataTypeCode;
            cloneField.BodyRequest = originalField.BodyRequest;
            cloneField.AdditionalHeadersText = originalField.AdditionalHeadersText;
        }
        protected override void Importing(ContentPart part, FieldExternal field, ImportContentContext context) {
            var BodyRequest = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "BodyRequest");
            if (BodyRequest != null) {
                field.BodyRequest = BodyRequest;
            }
            var ExternalUrl = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "ExternalUrl");
            if (ExternalUrl != null) {
                field.ExternalUrl = ExternalUrl;
            }
            var HttpDataTypeCode = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "HttpDataTypeCode");
            if (HttpDataTypeCode != null) {
                field.HttpDataTypeCode = HttpDataTypeCode;
            }
            var HttpVerbCode = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "HttpVerbCode");
            if (HttpVerbCode != null) {
                field.HttpVerbCode = HttpVerbCode;
            }
            var AdditionalHeadersText = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "AdditionalHeadersText");
            if (AdditionalHeadersText != null) {
                field.AdditionalHeadersText = AdditionalHeadersText;
            }
        }
    }
}