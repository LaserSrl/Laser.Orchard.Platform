using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoAlbum : VimeoBaseObject {
        public int duration { get; set; }
        public VimeoAlbumPrivacy privacy { get; set; }
        public VimeoAlbumMetadata metadata { get; set; }
    }

    public class VimeoAlbumPrivacy : VimeoBasePrivacy {
        
    }

    public class VimeoAlbumMetadata : VimeoBaseMetadata {

    }
}