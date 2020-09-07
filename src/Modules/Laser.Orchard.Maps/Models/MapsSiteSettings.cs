using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.Maps.Models {
    public class MapsSiteSettingsPart : ContentPart {
        public MapsProviders MapsProvider {
            get { return this.Retrieve(x => x.MapsProvider); }
            set { this.Store(x => x.MapsProvider, value); }
        }

        public string MapsTiles {
            get { return this.Retrieve(x => x.MapsTiles); }
            set { this.Store(x => x.MapsTiles, value); }
        }

        public string GoogleApiKey {
            get { return this.Retrieve(x => x.GoogleApiKey); }
            set { this.Store(x => x.GoogleApiKey, value); }
        }

        public int MaxZoom {
            get { return this.Retrieve(x => x.MaxZoom); }
            set { this.Store(x => x.MaxZoom, value); }
        }

        public bool KeepCultureConsistentWithContext {
            get { return this.Retrieve(x => x.KeepCultureConsistentWithContext); }
            set { this.Store(x => x.KeepCultureConsistentWithContext, value); }
        }

    }
}