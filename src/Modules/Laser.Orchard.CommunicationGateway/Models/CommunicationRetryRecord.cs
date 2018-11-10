using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Models {
    public class CommunicationRetryRecord {
        public virtual int Id { get; set; }
        public virtual int ContentItemRecord_Id { get; set; }
        public virtual string Context { get; set; }
        public virtual int NoOfFailures { get; set; }
        public virtual string Data { get; set; }
        public virtual bool PendingErrors { get; set; }
    }
}