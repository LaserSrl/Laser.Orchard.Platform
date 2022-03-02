using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.RazorBase.Models {
    public class KeyNameResolveContext {
        public string OriginalName { get; set; }
        public ResolveType ResolveType { get; set; }
        public ITemplateKey Context { get; set; }
        public string ResolvedName { get; set; }
        public bool NameResolved { get; set; }
    }
}