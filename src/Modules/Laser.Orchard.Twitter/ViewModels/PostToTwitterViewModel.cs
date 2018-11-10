using System;

namespace Laser.Orchard.Twitter.ViewModels {

    public class PostToTwitterViewModel {
        public string Message { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
        public Int32[] AccountList { get; set; }
    }
}