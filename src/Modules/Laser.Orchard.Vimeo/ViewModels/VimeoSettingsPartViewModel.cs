using Laser.Orchard.Vimeo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.ViewModels {
    public class VimeoSettingsPartViewModel {
        public string AccessToken { get; set; }
        public string ChannelName { get; set; }
        public string GroupName { get; set; }
        public string AlbumName { get; set; }

        public string License { get; set; } //get the corresponding options from CreativeCommonsOptions.txt
        public VimeoVideoPrivacy Privacy { get; set; } //this string is already a JSON representing a VimeoVideoPrivacy object
        public string Password { get; set; } //password for the case where privacy.view == password
        public bool ReviewLink { get; set; }
        public string Locale { get; set; } //set default language. Options are in LanguageCodes.txt
        public bool ContentRatingsSafe { get; set; }
        public Dictionary<string, bool> ContentRatingsUnsafe { get; set; }
        public string Whitelist { get; set; }

        public bool AlwaysUploadToGroup { get; set; }
        public bool AlwaysUploadToAlbum { get; set; }
        public bool AlwaysUploadToChannel { get; set; }

        public List<VimeoAccessTokenViewModel> AccessTokens { get; set; }
        public List<VimeoAccessTokenViewModel> DeletedAccessTokens { get; set; } //use this for updates

        public VimeoSettingsPartViewModel() {
            ContentRatingsUnsafe = new Dictionary<string, bool>();
            foreach (var cr in ContentRatingDictionary) {
                ContentRatingsUnsafe.Add(cr.Key, false);
            }
            AccessTokens = new List<VimeoAccessTokenViewModel>();
            DeletedAccessTokens = new List<VimeoAccessTokenViewModel>();
        }

        public VimeoSettingsPartViewModel(VimeoSettingsPart part) : this() {
            //AccessToken = part.AccessToken ?? "";
            ChannelName = part.ChannelName ?? "";
            GroupName = part.GroupName ?? "";
            AlbumName = part.AlbumName ?? "";

            License = part.License ?? "";
            Privacy = part.Privacy;
            Password = part.Password ?? "";
            ReviewLink = part.ReviewLink;
            Locale = part.Locale ?? "";
            if (part.ContentRatings.Count == 0 || part.ContentRatings.Contains("safe")) {
                ContentRatingsSafe = true;
            } else {
                foreach (var cr in ContentRatingDictionary) {
                    if (part.ContentRatings.Contains(cr.Key)) {
                        ContentRatingsUnsafe[cr.Key] = true;
                    }
                }
            }
            Whitelist = string.Join(", ", part.Whitelist);

            AlwaysUploadToGroup = part.AlwaysUploadToGroup;
            AlwaysUploadToAlbum = part.AlwaysUploadToAlbum;
            AlwaysUploadToChannel = part.AlwaysUploadToChannel;
        }

        //public string CensoredAccessToken {
        //    get {
        //        if (String.IsNullOrWhiteSpace(AccessToken))
        //            return "";
        //        else
        //            return AccessToken
        //                .Substring(AccessToken.Length - 4)
        //                .PadLeft(AccessToken.Length, 'X');
        //    }
        //}

        public static Dictionary<string, string> CCLicenseDictionary;
        public static Dictionary<string, string> LocaleDictionary;
        public static Dictionary<string, string> ContentRatingDictionary;
    }
}