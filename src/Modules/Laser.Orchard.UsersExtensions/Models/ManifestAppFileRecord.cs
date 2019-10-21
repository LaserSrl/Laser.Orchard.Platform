using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Models {
    public class ManifestAppFileRecord {
        public virtual int Id { get; set; }
        public virtual string FileContent { get; set; }
    }
}