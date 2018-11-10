using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class OpenAuthTemporaryUser : IUser {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }

        public ContentItem ContentItem {
            get {
                return null;
            }
        }

        public int Id {
            get {
                return 0;
            }
        }
    }
}