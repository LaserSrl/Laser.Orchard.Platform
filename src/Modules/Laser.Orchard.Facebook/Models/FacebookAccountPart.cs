using Orchard.ContentManagement;
using System;
using System.ComponentModel;

namespace Laser.Orchard.Facebook.Models {

    public class FacebookAccountPart : ContentPart {

        [DisplayName("SocialName")]
        public string SocialName {
            get { return this.Retrieve(r => r.SocialName); }
            set { this.Store(r => r.SocialName, value); }
        }

        [DisplayName("AccountType")]
        public string AccountType {
            get { return this.Retrieve(r => r.AccountType); }
            set { this.Store(r => r.AccountType, value); }
        }
        
        [DisplayName("UserToken")]
        public string UserToken {
            get { return this.Retrieve(r => r.UserToken); }
            set { this.Store(r => r.UserToken, value); }
        }

        [DisplayName("PageToken")]
        public string PageToken {
            get { return this.Retrieve(r => r.PageToken); }
            set { this.Store(r => r.PageToken, value); }
        }

        [DisplayName("IdPage")]
        public string IdPage {
            get { return this.Retrieve(r => r.IdPage); }
            set { this.Store(r => r.IdPage, value); }
        }

        [DisplayName("IdUser")]
        public Int32 IdUser {
            get { return this.Retrieve(r => r.IdUser); }
            set { this.Store(r => r.IdUser, value); }
        }

        [DisplayName("AccountShared")]
        public bool Shared {
            get { return this.Retrieve(r => r.Shared); }
            set { this.Store(r => r.Shared, value); }
        }

        [DisplayName("AccountShared")]
        public string PageName {
            get { return this.Retrieve(r => r.PageName); }
            set { this.Store(r => r.PageName, value); }
        }

        [DisplayName("Validated")]
        public bool Valid {
            get { return this.Retrieve(r => r.Valid); }
            set { this.Store(r => r.Valid, value); }
        }
        [DisplayName("DisplayAs")]
        public string DisplayAs {
            get { return this.Retrieve(r => r.DisplayAs); }
            set { this.Store(r => r.DisplayAs, value); }
        }

        public string UserIdFacebook {
            get { return this.Retrieve(r => r.UserIdFacebook); }
            set { this.Store(r => r.UserIdFacebook, value); }
        }
        public string UserName {
            get { return this.Retrieve(r => r.UserName); }
            set { this.Store(r => r.UserName, value); }
        }

     }

}