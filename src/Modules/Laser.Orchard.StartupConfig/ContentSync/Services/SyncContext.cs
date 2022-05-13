using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.ContentSync.Services {
    public class SyncContext {
        public SyncContext() {
            Target = new Target();
        }

        public ContentItem Source { get; set; }
        public Target Target { get; set; }
        public ContentItem Result { get; set; }
    }

    public class Target {
        public Target() {
            EnsureCreating = true;
            EnsurePublishing = true;
            EnsureVersioning = true;
        }

        public string Type { get; set; }
        public bool EnsureCreating { get; set; }
        public bool EnsurePublishing { get; set; }
        public bool EnsureVersioning { get; set; }
    }
}