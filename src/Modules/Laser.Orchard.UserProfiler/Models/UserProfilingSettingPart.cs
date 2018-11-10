using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Models {

    public class UserProfilingSettingPart : ContentPart {

        public int Range {
            get { return this.Retrieve(x => x.Range, 5); }
            set { this.Store(x => x.Range, value); }
        }

        public int RangeContentItem {
            get { return this.Retrieve(x => x.RangeContentItem, 0); }
            set { this.Store(x => x.RangeContentItem, value); }
        }
    }
}