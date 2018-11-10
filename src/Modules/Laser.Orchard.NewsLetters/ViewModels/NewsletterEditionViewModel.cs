using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;

namespace Laser.Orchard.NewsLetters.ViewModels {
    public class NewsletterEditionViewModel {
        public NewsletterEditionPart NewsletterEditionPart { get; set; } 
        public IList<AnnouncementLookupViewModel> AnnouncementToAttach { get; set; } 
    }
}