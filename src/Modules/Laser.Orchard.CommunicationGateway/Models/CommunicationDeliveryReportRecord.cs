using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Models {
    public class CommunicationDeliveryReportRecord {
        public virtual int Id { get; set; }
        public virtual int CommunicationAdvertisingPartRecord_Id { get; set; }
        public virtual string ExternalId { get; set; }
        // Date and time send request
        public virtual DateTime RequestDate { get; set; }
        // Date and time send
        public virtual DateTime SubmittedDate { get; set; }
        public virtual string Status { get; set; }
        public virtual string Recipient { get; set; }
        // Context: es. DriverId for SMS
        public virtual string Context { get; set; }
        // Medium: "SMS" or "PUSH" or "EMAIL"
        public virtual string Medium { get; set; }
    }
}