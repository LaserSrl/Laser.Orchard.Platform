using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Models {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperPart : ContentPart<KeywordHelperPartVersionRecord> {
        //this is a single string that will contain a comma separated list of keywords
        //When we build it from user input, we make sure that there are no leading or trailing white space.
        //we also make sure that multiple white spaces in a given keyword are removed
        public string Keywords {
            get { return this.Retrieve(x => x.Keywords); }
            set { this.Store(x => x.Keywords, value); }
        }
    }
}