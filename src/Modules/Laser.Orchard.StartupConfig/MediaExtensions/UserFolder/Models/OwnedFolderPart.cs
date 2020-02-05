using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.MediaExtensions.UserFolder.Models {
    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class OwnedFolderPart : ContentPart {
        public string FolderName {
            get { return this.Retrieve(x => x.FolderName); }
            set { this.Store(x => x.FolderName, value); }
        }
    }
}