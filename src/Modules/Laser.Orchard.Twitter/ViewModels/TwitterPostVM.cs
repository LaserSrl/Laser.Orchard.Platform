using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.ViewModels {

    public class TwitterPostVM {

        public TwitterPostVM() {
            ShowDescription = true;
            ShowTitle = true;
            ShowPicture = true;
        }

        public bool TwitterMessageSent { get; set; }
        public string TwitterMessage { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterPicture { get; set; }
        public string TwitterLink { get; set; }
       // public SelectList TwitterAccountList { get; set; }
        public List<OptionList> ListOption { get; set; }
        public string[] SelectedList { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowDescription { get; set; }
        public bool ShowPicture { get; set; }
        public bool TwitterCurrentLink { get; set; }
        public bool ShowTwitterCurrentLink { get; set; }
        public bool SendOnNextPublish { get; set; }
    }
    public class OptionList {
        public string Text { get; set; }
        public string Value { get; set; }
        public string ImageUrl { get; set; }
        public string Selected { get; set; }
    }
}