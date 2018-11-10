using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.NewsLetters.Models {
    public class NewsletterEditionPartRecord:ContentPartRecord {
        [Required]
        public virtual int NewsletterDefinitionPartRecord_Id { get; set; }
        [Required]
        public virtual int? Number { get; set; }
        [Required]
        public virtual bool Dispatched { get; set; }
        public virtual DateTime? DispatchDate { get; set; }
        public virtual string AnnouncementIds { get; set; }
    }
}