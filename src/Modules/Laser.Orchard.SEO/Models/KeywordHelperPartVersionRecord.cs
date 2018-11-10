using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Models {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperPartVersionRecord : ContentPartVersionRecord {
        //this is a single string that will contain a comma separated list of keywords
        public virtual string Keywords { get; set; }
    }
}