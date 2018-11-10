using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Mobile.ViewModels {
    public class UserAgentRedirectEdit {
        public UserAgentRedirectEdit() {
            Stores = new List<AppStoreEdit>();
        }
        public int Id { get; set; }
        public bool AutoRedirect { get; set; }
        [Required()]
        public string AppName { get; set; }
        public IList<AppStoreEdit> Stores { get; set; }

    }

    public class AppStoreEdit {
        public int Id { get; set; }
        [Required()]
        public int UserAgentRedirectPartRecord_Id { get; set; }
        public MobileAppStores AppStoreKey { get; set; }
        [Required()]
        //[RegularExpression(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?")]
        public string RedirectUrl { get; set; }
        public bool Delete { get; set; }
    }
}