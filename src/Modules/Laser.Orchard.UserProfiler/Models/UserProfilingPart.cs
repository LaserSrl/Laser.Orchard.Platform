using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Models {

    public class UserProfilingPart : ContentPart<UserProfilingPartRecord> {

        public string ListJson {
            get { return this.Retrieve(r => r.ListJson); }
            set { this.Store(r => r.ListJson, value); }
        }
    }

    public class UserProfilingPartRecord : ContentPartRecord {
        public virtual string ListJson { get; set; }
    }
}