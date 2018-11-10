using System.Collections.Generic;
using System.Web.Mvc;
using Laser.Orchard.Facebook.Models;

namespace Laser.Orchard.Facebook.ViewModels {
  
    public class FacebookPostVM {

        public FacebookPostVM() {
            ShowFacebookCaption = true;
            ShowFacebookDescription = true;
            ShowFacebookLink = true;
            ShowFacebookMessage = true;
            ShowFacebookName = true;
            ShowFacebookPicture = true;
        }

        public bool FacebookMessageSent { get; set; }
        public string FacebookMessage { get; set; }
        public string FacebookCaption { get; set; }
        public string FacebookDescription { get; set; }
        public string FacebookName { get; set; }
        public string FacebookPicture { get; set; }
        public string FacebookLink { get; set; }
        // public SelectList FacebookAccountList { get; set; }
        public List<OptionList> ListOption { get; set; }
        public string[] SelectedList { get; set; }
        public bool ShowFacebookCaption { get; set; }
        public bool ShowFacebookDescription { get; set; }
        public bool ShowFacebookLink { get; set; }
        public bool ShowFacebookMessage { get; set; }
        public bool ShowFacebookName { get; set; }
        public bool ShowFacebookPicture { get; set; }
        public bool SendOnNextPublish { get; set; }
        public FacebookType FacebookType { get; set; }
        public string FacebookMessageToPost { get; set; }
        public bool HasImage { get; set; }
    }

    public class OptionList {
        public string Text { get; set; }
        public string Value { get; set; }
        public string ImageUrl { get; set; }
        public string Selected { get; set; }
    }
}