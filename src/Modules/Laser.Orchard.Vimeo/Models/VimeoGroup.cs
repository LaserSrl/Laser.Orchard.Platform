using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoGroup : VimeoBaseObject {
        public string header { get; set; }
        public string resourceKey { get; set; }
    }

    public class VimeoGroupPrivacy : VimeoBasePrivacy {
        public string join { get; set; }
        public string videos { get; set; }
        public string comment { get; set; }
        public string forums { get; set; }
        public string invite { get; set; }
    }

    public class VimeoGroupMetadata : VimeoBaseMetadata {
        public Dictionary<string, VimeoBaseInteraction> interactions { get; set; }
    }

}