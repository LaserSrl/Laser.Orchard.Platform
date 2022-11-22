using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TenantBridges.Models {
    /// <summary>
    /// Use this part to configure the content we want to fetch from
    /// a remote tenant
    /// </summary>
    public class RemoteTenantContentSnippetWidgetPart : ContentPart {

        public string RemoteTenantBaseUrl {
            get { return this.Retrieve(x => x.RemoteTenantBaseUrl); }
            set { this.Store(x => x.RemoteTenantBaseUrl, value); }
        }

        // Should we fetch the content as object and try to render it here by
        // passing it through this tenant's @Display pipeline, or get an html
        // snippet directly prerendered?
        public bool ShouldGetHtmlSnippet {
            get { return this.Retrieve(x => x.ShouldGetHtmlSnippet); }
            set { this.Store(x => x.ShouldGetHtmlSnippet, value); }
        }

        #region Properties for when we are getting the HTML snippet
        public int RemoteContentId {
            get { return this.Retrieve(x => x.RemoteContentId); }
            set { this.Store(x => x.RemoteContentId, value); }
        }
        public bool RemoveRemoteWrappers {
            get { return this.Retrieve(x => x.RemoveRemoteWrappers); }
            set { this.Store(x => x.RemoveRemoteWrappers, value); }
        }
        // TODO: when it's actually implemented in the controller, add the Zone parameter
        #endregion

        #region Properties for when we are getting the serialized content
        public string Alias {
            get { return this.Retrieve(x => x.Alias); }
            set { this.Store(x => x.Alias, value); }
        }
        #endregion
    }
}