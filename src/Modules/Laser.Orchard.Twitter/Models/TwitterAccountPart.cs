using Orchard.ContentManagement;
using System;
using System.ComponentModel;

namespace Laser.Orchard.Twitter.Models {

    public class TwitterAccountPart : ContentPart {

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

        [DisplayName("UserTokenSecret")]
        public string UserTokenSecret {
            get { return this.Retrieve(r => r.UserTokenSecret); }
            set { this.Store(r => r.UserTokenSecret, value); }
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
    }
}