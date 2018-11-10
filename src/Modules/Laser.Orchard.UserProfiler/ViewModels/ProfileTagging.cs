using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.ViewModels {
    public class ProfileTagging {
        public TextSourceTypeOptions Type { get; set; } 
        public int UserId { get; set; }
        public string Text { get; set; }
        public int Count { get; set; }

        public ProfileTagging() {
            Type = TextSourceTypeOptions.Tag;
            Count = 1;
        }
    }
}