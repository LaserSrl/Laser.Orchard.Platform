using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.Models {
    public class NewsletterEditionPart : ContentPart<NewsletterEditionPartRecord> {
        public int NewsletterDefinitionPartRecord_Id {
            get { return Record.NewsletterDefinitionPartRecord_Id; }
            set { Record.NewsletterDefinitionPartRecord_Id = value; }
        }
        public int? Number {
            get { return Record.Number; }
            set { Record.Number = value; }
        }
        public bool Dispatched {
            get { return Record.Dispatched; }
            set { Record.Dispatched = value; }
        }
        public DateTime? DispatchDate {
            get { return Record.DispatchDate; }
            set { Record.DispatchDate = value; }
        }

        public string AnnouncementIds {
            get { return Record.AnnouncementIds; }
            set { Record.AnnouncementIds = value; }

        }
    }
}