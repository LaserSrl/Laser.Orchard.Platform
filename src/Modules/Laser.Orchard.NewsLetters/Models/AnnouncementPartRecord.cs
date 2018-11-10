using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.NewsLetters.Models {
    public class AnnouncementPartRecord : ContentPartRecord {
        public virtual string AnnouncementTitle { get; set; }
        public virtual string AttachToNextNewsletterIds { get; set; }
    }
}