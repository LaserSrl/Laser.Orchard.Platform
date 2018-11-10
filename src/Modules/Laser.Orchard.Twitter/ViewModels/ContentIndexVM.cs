using System;

namespace Laser.Orchard.Twitter.ViewModels {

    public class ContentIndexVM {
        public Int32 Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public DateTime? ModifiedUtc { get; set; }
        public dynamic Option { get; set; }
    }
}