using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.ViewModels {
    public class ProfileVM {
        public string Type { get; set; } 
        public string Text { get; set; }
        public int Count { get; set; }

        public ProfileVM() {
            Type = "Tag";
            Count = 1;
        }

    }

}