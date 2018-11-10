using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.NewsLetters.Models {
    public class NewsletterDefinitionPart : ContentPart<NewsletterDefinitionPartRecord> {
        public int TemplateRecord_Id {
            get { return Record.TemplateRecord_Id; }
            set { Record.TemplateRecord_Id = value; }
        }
        public int ConfirmSubscrptionTemplateRecord_Id {
            get { return Record.ConfirmSubscrptionTemplateRecord_Id; }
            set { Record.ConfirmSubscrptionTemplateRecord_Id = value; }
        }
        public int DeleteSubscrptionTemplateRecord_Id {
            get { return Record.DeleteSubscrptionTemplateRecord_Id; }
            set { Record.DeleteSubscrptionTemplateRecord_Id = value; }
        }

    }
}