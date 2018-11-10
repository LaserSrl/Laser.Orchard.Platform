using System;

namespace Laser.Orchard.Twitter.ViewModels {

    public class TwitterAccountVM {
        public string SocialName { get; set; }
        public string AccountType { get; set; }
        public string UserToken { get; set; }
        public string UserTokenSecret { get; set; }
        public Int32 IdUser { get; set; }
        public bool Shared { get; set; }
         public bool Valid { get; set; }
        public string DisplayAs { get; set; }
    }
}