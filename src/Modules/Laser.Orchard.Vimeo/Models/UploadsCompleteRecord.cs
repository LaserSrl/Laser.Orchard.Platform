using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class UploadsCompleteRecord {
        public virtual int Id { get; set; } //primary key provided because we do not inherit from ContentItemRecord
        public virtual string Uri { get; set; } //uri (relative to the base vimeo entrypoint) where our video can be reached
        public virtual int ProgressId { get; set; } //this is the Id this upload had when it was an upload in progress

        public virtual DateTime? CreatedTime { get; set; }

        public virtual bool Patched { get; set; }
        public virtual bool UploadedToGroup { get; set; }
        public virtual bool UploadedToChannel { get; set; }
        public virtual bool UploadedToAlbum { get; set; }
        public virtual bool IsAvailable { get; set; }

        public virtual int MediaPartId { get; set; }

        public virtual DateTime? ScheduledTerminationTime { get; set; } //when we will try to terminate the upload next
        public virtual bool MediaPartFinished { get; set; } //toggle this on once the media part gets finished
    }
}