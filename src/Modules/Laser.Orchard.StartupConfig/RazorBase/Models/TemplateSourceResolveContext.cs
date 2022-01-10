using RazorEngine.Templating;

namespace Laser.Orchard.StartupConfig.RazorBase.Models {
    public class TemplateSourceResolveContext {
        public ITemplateKey Key { get; set; }
        public string Source { get; set; }
        public string FileName { get; set; }
    }
}