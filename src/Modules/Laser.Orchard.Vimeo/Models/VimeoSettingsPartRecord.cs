using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoSettingsPartRecord : ContentPartRecord {
        //public virtual string AccessToken { get; set; }
        public virtual string ChannelName { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string AlbumName { get; set; }
        //The following settings are used to set default values for uploaded videos
        public virtual string License { get; set; } //get the corresponding options from CreativeCommonsOptions.txt
        public virtual string Privacy { get; set; } //this string is already a JSON representing a VimeoVideoPrivacy object
        public virtual string Password { get; set; } //password for the case where privacy.view == password
        public virtual bool ReviewLink { get; set; }
        public virtual string Locale { get; set; } //set default language. Options are in LanguageCodes.txt
        public virtual string ContentRatings { get; set; } //A JSON array containing the content rating options from ContentRating.txt
        //NOTE: ContentRating.txt does not contain 2 default ratings:
        //  safe,All Audiences
        //  unrated,Not Yet Rated
        public virtual string Whitelist { get; set; } //JSON array of domains where video embedding is enabled
        //TODO: Embed settings: thesee are the setting that have to do with the default appearance of the Vimeo player on web pages

        public virtual bool AlwaysUploadToGroup { get; set; }
        public virtual bool AlwaysUploadToAlbum { get; set; }
        public virtual bool AlwaysUploadToChannel { get; set; }

        //edit 2016/08/31: Optimization of API calls
        public virtual string AccountType { get; set; }
        public virtual DateTime? LastTimeAccountTypeWasChecked { get; set; }
        public virtual string UserId { get; set; }
        ////Limits on API calls
        //public virtual int RateLimitLimit { get; set; }
        //public virtual int RateLimitRemaining { get; set; }
        //public virtual DateTime? RateLimitReset { get; set; }
        //cache quota
        public virtual Int64 UploadQuotaSpaceFree { get; set; }
        public virtual Int64 UploadQuotaSpaceMax { get; set; }
        public virtual Int64 UploadQuotaSpaceUsed { get; set; }
        public virtual DateTime? LastTimeQuotaWasChecked { get; set; }
        //cache some ids
        public virtual string ChannelId { get; set; }
        public virtual string GroupId { get; set; }
        public virtual string AlbumId { get; set; }
    }
}