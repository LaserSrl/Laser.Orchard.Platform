using Laser.Orchard.TenantBridges.Models;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
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

        public string RemoteTenantBaseUrl { get; set; }
        public bool ShouldGetHtmlSnippet { get; set; }
        #region Properties for when we are getting the HTML snippet
        public int RemoteContentId { get; set; }
        public bool RemoveRemoteWrappers { get; set; }
        // TODO: when it's actually implemented in the controller, add the Zone parameter
        #endregion

        #region Properties for when we are getting the serialized content
        // TODO
        #endregion
    }
}