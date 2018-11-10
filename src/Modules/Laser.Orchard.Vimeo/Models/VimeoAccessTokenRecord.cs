using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoAccessTokenRecord {
        public virtual int Id { get; set; }
        public virtual string AccessToken { get; set; }
        public virtual int RateLimitLimit { get; set; }
        public virtual int RateLimitRemaining { get; set; }
        public virtual double RateAvailableRatio { get; set; }
        public virtual DateTime? RateLimitReset { get; set; }
    }
}