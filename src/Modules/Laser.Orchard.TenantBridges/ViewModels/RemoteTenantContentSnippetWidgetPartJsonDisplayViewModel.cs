using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TenantBridges.ViewModels {
    public class RemoteTenantContentSnippetWidgetPartJsonDisplayViewModel {

        public string Json { get; set; }
        public JObject JObject { get; set; }
    }
}