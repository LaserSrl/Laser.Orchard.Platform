using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.Models {
    public class AnnouncementPart : ContentPart<AnnouncementPartRecord> {

        public string AnnouncementTitle {
            get {
                return Record.AnnouncementTitle;
            }
            set {
                Record.AnnouncementTitle = value;
            }
        }

        public string AttachToNextNewsletterIds {
            get {
                return Record.AttachToNextNewsletterIds;
            }
            set {
                Record.AttachToNextNewsletterIds = value;
            }
        }
    }
}