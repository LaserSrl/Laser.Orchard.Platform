using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.NewsLetters.Models {
    public class NewsletterDefinitionPartRecord : ContentPartRecord{
        public virtual int TemplateRecord_Id { get; set; }
        public virtual int ConfirmSubscrptionTemplateRecord_Id { get; set; }
        public virtual int DeleteSubscrptionTemplateRecord_Id { get; set; }
    }
}