using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoPager {
        public int total { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
        public VimeoPaging paging { get; set; }
    }
    public class VimeoPaging {
        public string next { get; set; }
        public string previous { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }
}