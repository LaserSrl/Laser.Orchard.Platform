using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoPicture {
        public string uri { get; set; }
        public bool active { get; set; }
        public string type { get; set; }
        public List<VimeoPictureSizes> sizes { get; set; }
        public string resource_key { get; set; }
    }

    public class VimeoPictureSizes {
        public int width { get; set; }
        public int height { get; set; }
        public string link { get; set; }
        public string link_with_play_button { get; set; }
    }
}