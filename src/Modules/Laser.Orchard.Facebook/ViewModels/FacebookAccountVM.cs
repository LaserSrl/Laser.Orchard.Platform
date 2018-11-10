using System;

namespace Laser.Orchard.Facebook.ViewModels {

    public class FacebookAccountVM {
        public string SocialName { get; set; }
        public string UserToken { get; set; }
        public string PageToken { get; set; }
        public string IdPage { get; set; }
        public Int32 IdUser { get; set; }
        public bool Shared { get; set; }
        public string PageName { get; set; }
        public bool Valid { get; set; }
        public string DisplayAs { get; set; }
        public string AccountType { get; set; }
        public string UserIdFacebook { get; set; }
        public string UserName { get; set; }
    }
}