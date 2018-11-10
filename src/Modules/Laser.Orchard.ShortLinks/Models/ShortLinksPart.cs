using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Laser.Orchard.ShortLinks.Models {

    public class ShortLinksRecord : ContentPartRecord {
        public virtual string Url { get; set; }
        public virtual string FullLink { get; set; }       
    }

    public class ShortLinksPart : ContentPart<ShortLinksRecord> {
        
        
        [DisplayName("Url")]
        public string Url {
            get { return this.Retrieve(r => r.Url); }
            set { this.Store(r => r.Url, value); }
        }

        [DisplayName("FullLink")]
        public string FullLink {
            get { return this.Retrieve(r => r.FullLink); }
            set { this.Store(r => r.FullLink, value); }
        }


    }

    public class ShortLinksSettingsPart : ContentPart {
               
        [DisplayName("FirebaseApiKey")]
        public string GoogleApiKey {
            get { return this.Retrieve(r => r.GoogleApiKey); }
            set { this.Store(r => r.GoogleApiKey, value); }
        }

        [DisplayName("Dynamic  LinkDomain")]
        public string DynamicLinkDomain {
            get { return this.Retrieve(r => r.DynamicLinkDomain); }
            set { this.Store(r => r.DynamicLinkDomain, value); }
        }

        [DisplayName("Has sensitive data")]
        public bool HasSensitiveData {
            get { return this.Retrieve(r => r.HasSensitiveData); }
            set { this.Store(r => r.HasSensitiveData, value); }
        }


    }
}
