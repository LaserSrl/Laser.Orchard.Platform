using System;
using Laser.Orchard.SecureData.Fields;
using Laser.Orchard.SecureData.Settings;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.SecureData.Security;
using Orchard.Security;
using Laser.Orchard.SecureData.ViewModels;
using System.Text.RegularExpressions;
using Laser.Orchard.SecureData.Services;

namespace Laser.Orchard.SecureData.Drivers {
    public class EncryptedStringFieldDriver : ContentFieldDriver<EncryptedStringField> {
        private readonly IAuthorizer _authorizer;
        private readonly ISecureFieldService _secureFieldService;

        // Variables to set the templates to show in the content editor (backoffice).
        private readonly string TemplateNameAuthorized = "Fields/EncryptedStringField.Edit";
        private readonly string TemplateNameUnauthorized = "Fields/EncryptedStringField.Unauthorized";
        private readonly string ShapeType = "Fields_EncryptedString_Edit";

        public EncryptedStringFieldDriver(IAuthorizer authorizer, ISecureFieldService secureFieldService) {
            _authorizer = authorizer;
            _secureFieldService = secureFieldService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        public static string GetDifferentiator(EncryptedStringField field, ContentPart part) {
            return field.Name;
        }

        // This function shows the view for the field.
        protected override DriverResult Editor(ContentPart part, EncryptedStringField field, dynamic shapeHelper) {
            // Permission management.
            if (AuthorizeEdit(part, field)) {
                // If user is authorized, the following code is showing the view.
                // TemplateName: TemplateNameAuthorized -> it's the actual view with the edit form.
                return ContentShapeFromViewModel(part, field, TemplateNameAuthorized, CreateViewModel(field), shapeHelper);
            } else {
                // If user is unauthorized, I show nothing.
                // TemplateName: TemplateNameUnauthorized -> it's an empty view; Model: null.
                return ContentShapeFromViewModel(part, field, TemplateNameUnauthorized, null, shapeHelper);
            }
        }

        // This function saves the field after a post, then shows the field form again.
        protected override DriverResult Editor(ContentPart part, EncryptedStringField field, IUpdateModel updater, dynamic shapeHelper) {
            if (AuthorizeEdit(part, field)) {
                var viewModel = new EncryptedStringFieldEditViewModel(field.PartFieldDefinition.Settings.GetModel<EncryptedStringFieldSettings>());
                string prefix = GetPrefix(field, part);

                if (updater.TryUpdateModel(viewModel, prefix, null, null)) {
                    if (Validate(viewModel, field, prefix, updater)) {
                        _secureFieldService.EncodeValue(field, viewModel.Value);
                    } else {
                        return ContentShapeFromViewModel(part, field, TemplateNameAuthorized, viewModel, shapeHelper);
                    }
                }
            }

            // Call the function to show the view.
            return Editor(part, field, shapeHelper);
        }

        private DriverResult ContentShapeFromViewModel(ContentPart part, EncryptedStringField field, string templateName, EncryptedStringFieldEditViewModel viewModel, dynamic shapeHelper) {
            return ContentShape(ShapeType, GetDifferentiator(field, part),
                    () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: templateName,
                            Model: viewModel,
                            Prefix: GetPrefix(field, part));
                    });
        }

        private bool Validate(EncryptedStringFieldEditViewModel viewModel, EncryptedStringField field, string prefix, IUpdateModel updater) {
            var settings = field.PartFieldDefinition.Settings.GetModel<EncryptedStringFieldSettings>();

            if (settings.Required) {
                if (settings.IsVisible && string.IsNullOrWhiteSpace(viewModel.Value)) {
                    updater.AddModelError(prefix, T("The field {0} is mandatory.", T(field.DisplayName)));
                    return false;
                }

                if (!settings.IsVisible && string.IsNullOrWhiteSpace(viewModel.Value) && string.IsNullOrWhiteSpace(field.Value)) {
                    updater.AddModelError(prefix, T("The field {0} is mandatory.", T(field.DisplayName)));
                    return false;
                }

                if (!settings.IsVisible && string.IsNullOrWhiteSpace(viewModel.Value) && !string.IsNullOrWhiteSpace(field.Value)) {
                    // Keep the Value already saved.
                    return false;
                }
            }

            if (!settings.IsVisible && string.IsNullOrWhiteSpace(viewModel.Value) && !string.IsNullOrWhiteSpace(field.Value) && !viewModel.SaveIfEmpty) {
                // Keep the Value already saved.
                return false;
            }

            if (settings.ConfirmRequired && !viewModel.Value.Equals(viewModel.ConfirmValue)) {
                updater.AddModelError(prefix, T("The value of the field {0} must match the Confirm value.", T(field.DisplayName), settings.Pattern));
                return false;
            }

            if (!CheckPattern(viewModel.Value, settings.Pattern)) {
                updater.AddModelError(prefix, T("The value of the field {0} is not valid." + Environment.NewLine + "Pattern: {1}", T(field.DisplayName), settings.Pattern));
                return false;
            }

            return true;
        }

        private bool CheckPattern(string value, string pattern) {
            if (string.IsNullOrWhiteSpace(pattern)) {
                return true;
            }

            return Regex.IsMatch(value, pattern, RegexOptions.Compiled);
        }

        private bool AuthorizeEdit(ContentPart part, EncryptedStringField field) {
            // TODO: check authorizations on fields.
            return _authorizer.Authorize(_secureFieldService.GetOwnPermission(part, field), part);
        }

        private EncryptedStringFieldEditViewModel CreateViewModel(EncryptedStringField field) {
            var settings = field.PartFieldDefinition.Settings.GetModel<EncryptedStringFieldSettings>();

            var vm = new EncryptedStringFieldEditViewModel(settings) {
                //Settings = settings,
                DisplayName = field.PartFieldDefinition.DisplayName,
                HasValue = !string.IsNullOrWhiteSpace(field.Value)
            };

            // Show the value if it's visible only.
            if (settings.IsVisible) {
                vm.Value = GetDecryptedValue(field);
            }

            return vm;
        }

        private string GetDecryptedValue(EncryptedStringField field) {
            return _secureFieldService.DecodeValue(field);
        }

        // This routine is used to search content.
        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The value of the field."))
                // TODO: Decode the encrypted value.
                .Enumerate<EncryptedStringField>(() => field => new[] { field.Value });
        }
    }
}