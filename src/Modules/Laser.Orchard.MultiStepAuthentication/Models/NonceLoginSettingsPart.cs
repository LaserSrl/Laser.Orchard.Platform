using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Models {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginSettingsPart : ContentPart {
        public int NonceMinutesValidity {
            get { return this.Retrieve(x => x.NonceMinutesValidity); }
            set { this.Store(x => x.NonceMinutesValidity, value); }
        }
        public string LoginLinkFormat {
            get { return this.Retrieve(x => x.LoginLinkFormat); }
            set { this.Store(x => x.LoginLinkFormat, value); }
        }
    }
}