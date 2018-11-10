using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Models {

    public class UserProfilingSummaryRecord {
        public virtual int Id { get; set; }
        public virtual UserProfilingPartRecord UserProfilingPartRecord { get; set; }
        public virtual string Text { get; set; }
        public virtual string Data { get; set; }
        public virtual TextSourceTypeOptions SourceType { get; set; }
        public virtual int Count { get; set; }

        public UserProfilingSummaryRecord() {
            this.Count = 0;
            this.SourceType = TextSourceTypeOptions.Tag;
            this.Data = "";
        }
    }
}