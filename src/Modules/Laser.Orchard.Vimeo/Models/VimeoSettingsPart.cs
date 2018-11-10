using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoSettingsPart : ContentPart<VimeoSettingsPartRecord> {
        //public string AccessToken {
        //    get { return Record.AccessToken; }
        //    set { Record.AccessToken = value; }
        //}
        public string ChannelName {
            get { return Record.ChannelName; }
            set { Record.ChannelName = value; }
        }
        public string GroupName {
            get { return Record.GroupName; }
            set { Record.GroupName = value; }
        }
        public string AlbumName {
            get { return Record.AlbumName; }
            set { Record.AlbumName = value; }
        }

        //The following settings are used to set default values for uploaded videos
        public string License {
            get { return Record.License; }
            set { Record.License = value; }
        }
        public VimeoVideoPrivacy Privacy {
            get {
                return Record.Privacy != null ?
                    JsonConvert.DeserializeObject<VimeoVideoPrivacy>(Record.Privacy) :
                    new VimeoVideoPrivacy();
            }
            set {
                Record.Privacy = JsonConvert.SerializeObject(value);
            }
        }
        public string Password {
            get { return Record.Password; }
            set { Record.Password = value; }
        }
        public bool ReviewLink {
            get { return Record.ReviewLink; }
            set { Record.ReviewLink = value; }
        }
        public string Locale {
            get { return Record.Locale; }
            set { Record.Locale = value; }
        }
        public List<string> ContentRatings {
            get {
                return Record.ContentRatings != null ?
                    JsonConvert.DeserializeObject<List<string>>(Record.ContentRatings) :
                    new List<string>();
            }
            set {
                Record.ContentRatings = JsonConvert.SerializeObject(value);
            }
        }
        public List<string> Whitelist {
            get {
                return Record.Whitelist != null ?
                    JsonConvert.DeserializeObject<List<string>>(Record.Whitelist) :
                    new List<string>();
            }
            set {
                Record.Whitelist = JsonConvert.SerializeObject(value);
            }
        }

        public bool AlwaysUploadToGroup {
            get { return Record.AlwaysUploadToGroup; }
            set { Record.AlwaysUploadToGroup = value; }
        }
        public bool AlwaysUploadToAlbum {
            get { return Record.AlwaysUploadToAlbum; }
            set { Record.AlwaysUploadToAlbum = value; }
        }
        public bool AlwaysUploadToChannel {
            get { return Record.AlwaysUploadToChannel; }
            set { Record.AlwaysUploadToChannel = value; }
        }

        //edit 2016/08/31: Optimization of API calls
        //store account type to avoid checking for it every time we need to retrieve an URL
        public string AccountType {
            get { return Record.AccountType; }
            set { Record.AccountType = value; }
        }
        //Save the time when we most recently retrieved the account type from Vimeo.
        public DateTime? LastTimeAccountTypeWasChecked {
            get { return Record.LastTimeAccountTypeWasChecked; }
            set { Record.LastTimeAccountTypeWasChecked = value; }
        }
        //store the user Id associated with the Vimeo account
        public string UserId {
            get { return Record.UserId; }
            set { Record.UserId = value; }
        }
        ////Total number of API requests we are allowed to make in an hour
        //public int RateLimitLimit {
        //    get { return Record.RateLimitLimit; }
        //    set { Record.RateLimitLimit = value; }
        //}
        ////Number of API requests we can still make before the next reset
        //public int RateLimitRemaining {
        //    get { return Record.RateLimitRemaining; }
        //    set { Record.RateLimitRemaining = value; }
        //}
        ////Timestamp of when the rate limit will reset next
        //public DateTime? RateLimitReset {
        //    get { return Record.RateLimitReset; }
        //    set { Record.RateLimitReset = value; }
        //}
        //cache for upload quota
        public Int64 UploadQuotaSpaceFree {
            get { return Record.UploadQuotaSpaceFree; }
            set { Record.UploadQuotaSpaceFree = value; }
        }
        public Int64 UploadQuotaSpaceMax {
            get { return Record.UploadQuotaSpaceMax; }
            set { Record.UploadQuotaSpaceMax = value; }
        }
        public Int64 UploadQuotaSpaceUsed {
            get { return Record.UploadQuotaSpaceUsed; }
            set { Record.UploadQuotaSpaceUsed = value; }
        }
        public DateTime? LastTimeQuotaWasChecked {
            get { return Record.LastTimeQuotaWasChecked; }
            set { Record.LastTimeQuotaWasChecked = value; }
        }
        //store the user Ids associated with the categories
        public string ChannelId {
            get { return Record.ChannelId; }
            set { Record.ChannelId = value; }
        }
        public string GroupId {
            get { return Record.GroupId; }
            set { Record.GroupId = value; }
        }
        public string AlbumId {
            get { return Record.AlbumId; }
            set { Record.AlbumId = value; }
        }
    }
}