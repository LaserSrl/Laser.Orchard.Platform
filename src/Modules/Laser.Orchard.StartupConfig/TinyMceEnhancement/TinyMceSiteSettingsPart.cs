using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.StartupConfig.TinyMceEnhancement {
    [OrchardFeature("Laser.Orchard.StartupConfig.TinyMceEnhancement")]
    public class TinyMceSiteSettingsPart : ContentPart {
        public string InitScript {
            get { return this.Retrieve(x => x.InitScript); }
            set { this.Store(x => x.InitScript, value); }
        }
        public string FrontendInitScript {
            get { return this.Retrieve(x => x.FrontendInitScript); }
            set { this.Store(x => x.FrontendInitScript, value); }
        }
        public string AdditionalPlugins {
            get { return this.Retrieve(x => x.AdditionalPlugins); }
            set { this.Store(x => x.AdditionalPlugins, value); }
        }
        public string FrontendAdditionalPlugins {
            get { return this.Retrieve(x => x.FrontendAdditionalPlugins); }
            set { this.Store(x => x.FrontendAdditionalPlugins, value); }
        }
        // property not to be saved on repository
        public string DefaultInitScript { get; set; }
        public string FrontendDefaultInitScript { get; set; }
    }
}