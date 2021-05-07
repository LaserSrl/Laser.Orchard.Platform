using Laser.Orchard.SecureData.Fields;
using Laser.Orchard.SecureData.Services;
using Laser.Orchard.SecureData.Settings;
using Laser.Orchard.SecureData.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.SecureData.Drivers {
    public class HashedStringFieldDriver : ContentFieldCloningDriver<HashedStringField> {
        private IAuthorizer _authorizer;
        private ISecureFieldService _secureFieldService;

        // Variables to set the templates to show in the content editor (backoffice).
        private static string TemplateNameAuthorized = "Fields/HashStringField.Edit";
        private static string TemplateNameUnauthorized = "Fields/HasStringField.Unauthorized";
        private static string ShapeType = "Fields_HashString_Edit";

        public Localizer T { get; set; }

        public HashedStringFieldDriver(IAuthorizer authorizer, ISecureFieldService secureFieldService) {
            _authorizer = authorizer;
            _secureFieldService = secureFieldService;

            T = NullLocalizer.Instance;
        }

        public static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        public static string GetDifferentiator(HashedStringField field, ContentPart part) {
            return field.Name;
        }

        // This function shows the view for the field.
        protected override DriverResult Editor(ContentPart part, HashedStringField field, dynamic shapeHelper) {
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
        protected override DriverResult Editor(ContentPart part, HashedStringField field, IUpdateModel updater, dynamic shapeHelper) {
            if (AuthorizeEdit(part, field)) {
                var viewModel = new HashedStringFieldEditViewModel(field.PartFieldDefinition.Settings.GetModel<HashedStringFieldSettings>());
                string prefix = GetPrefix(field, part);

                if (updater.TryUpdateModel(viewModel, prefix, null, null)) {
                    if (Validate(viewModel, field, prefix, updater)) {
                        _secureFieldService.HashValue(field, viewModel.Value);
                    } else {
                        return ContentShapeFromViewModel(part, field, TemplateNameAuthorized, viewModel, shapeHelper);
                    }
                }
            }

            // Call the function to show the view.
            return Editor(part, field, shapeHelper);
        }

        private DriverResult ContentShapeFromViewModel(ContentPart part, HashedStringField field, string templateName, HashedStringFieldEditViewModel viewModel, dynamic shapeHelper) {
            return ContentShape(ShapeType, GetDifferentiator(field, part),
                    () => {
                        return shapeHelper.EditorTemplate(
                            TemplateName: templateName,
                            Model: viewModel,
                            Prefix: GetPrefix(field, part));
                    });
        }

        private bool Validate(HashedStringFieldEditViewModel viewModel, HashedStringField field, string prefix, IUpdateModel updater) {
            var settings = field.PartFieldDefinition.Settings.GetModel<HashedStringFieldSettings>();

            if (settings.Required) {
                // If there is no previous Value and there is no Value in the viewModel.
                if (string.IsNullOrWhiteSpace(field.Value) && string.IsNullOrWhiteSpace(viewModel.Value)) {
                    updater.AddModelError(prefix, T("The field {0} is mandatory.", T(field.DisplayName)));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(viewModel.Value) && !string.IsNullOrWhiteSpace(field.Value)) {
                    // Keep the already saved Value without showing a error.
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(viewModel.Value) && !string.IsNullOrWhiteSpace(field.Value) && !viewModel.SaveIfEmpty) {
                // Keep the already saved Value without showing a error.
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

        private bool AuthorizeEdit(ContentPart part, ContentField field) {
            return true;
        }

        private HashedStringFieldEditViewModel CreateViewModel(HashedStringField field) {
            var settings = field.PartFieldDefinition.Settings.GetModel<HashedStringFieldSettings>();

            var vm = new HashedStringFieldEditViewModel(settings) {
                //Settings = settings,
                DisplayName = field.PartFieldDefinition.DisplayName,
                HasValue = !string.IsNullOrWhiteSpace(field.Value)
            };
            
            return vm;
        }
    }
}