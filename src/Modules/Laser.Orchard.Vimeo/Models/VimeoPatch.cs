using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoPatch {
        public string name { get; set; }
        public string description { get; set; }
        public string license { get; set; }
        public VimeoVideoPrivacy privacy { get; set; }
        public string password { get; set; }
        public bool review_link { get; set; }
        public string locale { get; set; }
        public List<string> content_rating { get; set; }

    }
}