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

namespace Laser.Orchard.SecureData.Drivers {
    public class EncryptedStringFieldDriver : ContentFieldDriver<EncryptedStringField> {
        private readonly IAuthorizer _authorizer;
        // Variables to set the templates to show in the content editor (backoffice).
        private readonly string TemplateNameAuthorized = "Fields/EncryptedStringField.Edit";
        private readonly string TemplateNameUnauthorized = "Fields/EncryptedStringField.Unauthorized";

        public EncryptedStringFieldDriver(IAuthorizer authorizer) {
            _authorizer = authorizer;

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
                return ContentShape("Fields_EncryptedString_Edit", GetDifferentiator(field, part),
                    () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: TemplateNameAuthorized,
                            Model: CreateViewModel(field),
                            Prefix: GetPrefix(field, part));
                    });
            } else {
                // If user is unauthorized, I show nothing.
                // TemplateName: TemplateNameUnauthorized -> it's an empty view; Model: null.
                return ContentShape("Fields_EncryptedString_Edit", GetDifferentiator(field, part),
                    () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: TemplateNameUnauthorized,
                            Model: null,
                            Prefix: GetPrefix(field, part));
                    });
            }
        }

        // This function saves the field after a post, then shows the field form again.
        protected override DriverResult Editor(ContentPart part, EncryptedStringField field, IUpdateModel updater, dynamic shapeHelper) {
            if (AuthorizeEdit(part, field)) {
                var viewModel = new EncryptedStringFieldEditViewModel();
                string prefix = GetPrefix(field, part);

                if (updater.TryUpdateModel(viewModel, prefix, null, null)) {
                    var settings = field.PartFieldDefinition.Settings.GetModel<EncryptedStringFieldSettings>();

                    // If field is mandatory, check if both Value and ConfirmValue contain a value.
                    // Field.Value -> data I have already save on database.
                    // ViewModel.Value -> data sent by the edit form.
                    // If field.Value is empty the field is a new record on database or the record has been saved with no value.
                    if (settings.Required && string.IsNullOrWhiteSpace(viewModel.Value) && string.IsNullOrWhiteSpace(field.Value)) {
                        updater.AddModelError(prefix, T("The field '{0}' is mandatory.", T(field.DisplayName)));
                    } else {
                        // Check if Value and Confirm Value contain the same value.
                        if (settings.ConfirmRequired && !viewModel.Value.Equals(viewModel.ConfirmValue)) {
                            updater.AddModelError(prefix, T("'{0}' and 'Confirm {0}' must contain the same value.", T(field.DisplayName)));
                        } else {
                            field.Value = viewModel.Value;
                        }
                    }
                }
            }

            // Call the function to show the view.
            return Editor(part, field, shapeHelper);
        }

        private bool AuthorizeEdit(ContentPart part, EncryptedStringField field) {
            return _authorizer.Authorize(EncryptedStringFieldPermissions.ManageAllEncryptedStringFields);
        }

        private EncryptedStringFieldEditViewModel CreateViewModel(EncryptedStringField field) {
            var settings = field.PartFieldDefinition.Settings.GetModel<EncryptedStringFieldSettings>();

            var vm = new EncryptedStringFieldEditViewModel {
                Settings = settings,
                DisplayName = field.PartFieldDefinition.DisplayName
            };

            // Show the value if it's visible only.
            if (settings.IsVisible) {
                vm.Value = GetDecryptedValue(field);
            }

            return vm;
        }

        // This routine is used to search content.
        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The value of the field."))
                // TODO: Decode the encrypted value.
                .Enumerate<EncryptedStringField>(() => field => new[] { field.Value });
        }

        private string GetDecryptedValue(EncryptedStringField field) {
            // TODO: decrypt the value.
            return field.Value;
        }
    }
}