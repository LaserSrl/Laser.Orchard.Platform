using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoUser : VimeoBaseObject {
        public string location { get; set; }
        public string bio { get; set; }
        public string account { get; set; }
        public List<VimeoWebsite> websites { get; set; }
        public VimeoUserMetadata metadata { get; set; }
        public VimeoUserPreferences preferences { get; set; }
        public List<string> contentFilter { get; set; }
        public string resourceKey { get; set; }
        public VimeoUploadQuota upload_quota { get; set; }
    }

    public class VimeoUserMetadata : VimeoBaseMetadata {

    }

    public class VimeoUserPreferences {
        public Dictionary<string, string> videos { get; set; }
    }

    public class VimeoUploadQuota {
        public VimeoSpace space { get; set; }
        public VimeoQuota quota { get; set; }

        public VimeoUploadQuota() {
            space = new VimeoSpace();
            quota = new VimeoQuota();
        }

        public VimeoUploadQuota(Int64 ff, Int64 mm, Int64 uu) {
            space = new VimeoSpace(ff, mm, uu);
            quota = new VimeoQuota();
        }
    }
    public class VimeoSpace {
        public Int64 free { get; set; }
        public Int64 max { get; set; }
        public Int64 used { get; set; }

        public VimeoSpace() {
            free = 0;
            max = 0;
            used = 0;
        }

        public VimeoSpace(Int64 f, Int64 m, Int64 u) {
            free = f;
            max = m;
            used = u;
        }
    }
    public class VimeoQuota {
        public bool hd { get; set; }
        public bool sd { get; set; }
    }
}