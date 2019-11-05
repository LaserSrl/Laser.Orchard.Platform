using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceSiteSettingsPart : ContentPart {
        public string InitScript {
            get { return this.Retrieve(x => x.InitScript); }
            set { this.Store(x => x.InitScript, value); }
        }
        public string AdditionalPlugins {
            get { return this.Retrieve(x => x.AdditionalPlugins); }
            set { this.Store(x => x.AdditionalPlugins, value); }
        }
        // property not to be saved on repository
        public string DefaultInitScript { get; set; }
    }
}