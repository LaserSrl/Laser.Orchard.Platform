using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoVideo : VimeoBaseObject {
        public int duration { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string language { get; set; }
        public VimeoVideoEmbed embed { get; set; }
        public string release_time { get; set; }
        public List<string> content_rating { get; set; }
        public string license { get; set; }
        public VimeoVideoPrivacy privacy { get; set; }
        public List<VimeoVideoTag> tags { get; set; }
        public VimeoVideoStats stats { get; set; }
        public VimeoVideoMetadata metadata { get; set; }
        public VimeoBaseObject app { get; set; } //TODO: verify this, because in the videos we tested they were null
        public string status { get; set; }
        public string resource_key { get; set; }
        public string embed_presets { get; set; } //TODO: verify this, because in the videos we tested they were null
    }

    public class VimeoVideoEmbed {
        public string uri { get; set; }
        public string html { get; set; }
        public Dictionary<string, bool> buttons { get; set; }
        public VimeoVideoEmbedLogos logos { get; set; }
        public Dictionary<string, string> title { get; set; }
        public bool playbar { get; set; }
        public bool volume { get; set; }
        public string color { get; set; }
    }

    public class VimeoVideoEmbedLogos {
        public bool vimeo { get; set; }
        public VimeoVideoEmbedCustomLogo custom { get; set; }
    }

    public class VimeoVideoEmbedCustomLogo {
        public bool active { get; set; }
        public string link { get; set; }
        public bool sticky { get; set; }
    }

    public class VimeoVideoPrivacy : VimeoBasePrivacy {
        public string embed { get; set; }
        public bool download { get; set; }
        public bool add { get; set; }
        public string comments { get; set; }
    }

    public class VimeoVideoTag {
        public string uri { get; set; }
        public string name { get; set; }
        public string tag { get; set; }
        public string canonical { get; set; }
        public VimeoBaseMetadata metadata { get; set; }
        public string resource_key { get; set; }
    }

    public class VimeoVideoStats {
        public int plays { get; set; }
        public int likes { get; set; }
    }

    public class VimeoVideoMetadata : VimeoBaseMetadata {
        public Dictionary<string, VimeoBaseInteraction> interactions { get; set; }
    }
}