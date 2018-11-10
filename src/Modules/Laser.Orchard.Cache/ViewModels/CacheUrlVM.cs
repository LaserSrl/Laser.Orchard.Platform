using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Cache.Models;

namespace Laser.Orchard.Cache.ViewModels {
    public class CacheUrlVM {
       public IEnumerable<CacheUrlRecord> Cached { get; set; }
    }

}