using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NewsLetters.ViewModels {
    public class AnnouncementLookupViewModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }
    }
}