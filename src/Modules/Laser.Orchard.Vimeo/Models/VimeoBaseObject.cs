using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoBaseObject {
        public string uri { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string created_time { get; set; }
        public string modified_time { get; set; }
        public VimeoUser user { get; set; }
        public VimeoPicture pictures { get; set; }
    }

    public class VimeoBasePrivacy {
        public string view { get; set; }
    }

    public class VimeoBaseMetadata {
        public Dictionary<string, VimeoBaseMetadataConnection> connections { get; set; }
         
    }

    public class VimeoBaseMetadataConnection {
        public string uri { get; set; }
        public List<String> methods { get; set; }
    }

    public class VimoeMetadataConnectionTotal : VimeoBaseMetadataConnection {
        public int total { get; set; }
    }
    public class VimeoMetadataConnectionUsers : VimeoBaseMetadataConnection {
        public int users { get; set; }
    }

    public class VimeoBaseInteraction {
        public bool added { get; set; }
        public string added_time { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string uri { get; set; }
    }
}