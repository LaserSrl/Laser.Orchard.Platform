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
    }
}