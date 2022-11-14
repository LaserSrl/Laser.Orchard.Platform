using Laser.Orchard.TenantBridges.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TenantBridges.ViewModels {
    public class RemoteTenantContentSnippetWidgetPartEditorViewModel {

        protected RemoteTenantContentSnippetWidgetPartEditorViewModel() {

        }

        public static RemoteTenantContentSnippetWidgetPartEditorViewModel
            FromPart(RemoteTenantContentSnippetWidgetPart part) {
            var vm = new RemoteTenantContentSnippetWidgetPartEditorViewModel();
            vm.RemoteTenantBaseUrl = part.RemoteTenantBaseUrl;
            vm.ShouldGetHtmlSnippet = part.ShouldGetHtmlSnippet;
            vm.RemoteContentId = part.RemoteContentId;
            vm.RemoveRemoteWrappers = part.RemoveRemoteWrappers;
            return vm;
        }

        public void UpdatePart(RemoteTenantContentSnippetWidgetPart part) {
            part.RemoteTenantBaseUrl = RemoteTenantBaseUrl;
            part.ShouldGetHtmlSnippet = ShouldGetHtmlSnippet;
            part.RemoteContentId = RemoteContentId;
            part.RemoveRemoteWrappers = RemoveRemoteWrappers;
        }
        [Required]
        public string RemoteTenantBaseUrl { get; set; }
        public bool ShouldGetHtmlSnippet { get; set; }
        #region Properties for when we are getting the HTML snippet
        [ValidateWidgetPartEditorProperty]
        public int RemoteContentId { get; set; }
        public bool RemoveRemoteWrappers { get; set; }
        // TODO: when it's actually implemented in the controller, add the Zone parameter
        #endregion

        #region Properties for when we are getting the serialized content
        // TODO
        #endregion
    }
    /// <summary>
    /// This validator is to apply specific logic on some properties of the viewmodel
    /// </summary>
    public class ValidateWidgetPartEditorProperty : ValidationAttribute {
        public ValidateWidgetPartEditorProperty() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext) {

            var model = (RemoteTenantContentSnippetWidgetPartEditorViewModel)
                validationContext.ObjectInstance;

            if (model.ShouldGetHtmlSnippet) {
                // Id should be greater than 0
                if (validationContext.DisplayName == "RemoteContentId") {
                    if (model.RemoteContentId <= 0) {
                        return new ValidationResult(T("Remote content Id must be greater than 0.").Text);
                    }
                }
            }
            else {

            }

            return ValidationResult.Success;
        }
    }
}