using Orchard.ContentManagement;

namespace Laser.Orchard.WebServices.Models {

    public class WebServiceSettingsPart : ContentPart {

        public bool LogWebservice {
            get { return this.Retrieve(x => x.LogWebservice, false); }
            set { this.Store(x => x.LogWebservice, value); }
        }
    }
}