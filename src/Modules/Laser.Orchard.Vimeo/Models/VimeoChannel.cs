using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoChannel : VimeoBaseObject {
        public string header { get; set; }
        public VimeoChannelPrivacy privacy { get; set; }
        public VimeoChannelMetadata metadata { get; set; }
        public string resource_key { get; set; }
    }

    public class VimeoChannelPrivacy : VimeoBasePrivacy {
        
    }

    public class VimeoChannelMetadata : VimeoBaseMetadata {
        public Dictionary<string, VimeoBaseInteraction> interactions { get; set; }
    }
}