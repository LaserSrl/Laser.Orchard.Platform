using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Models {
    public class BannerAgentPart : ContentPart<BannerAgentPartRecord> {
        public string title {
            get{return this.Retrieve(r => r.title);}
            set{this.Store(r => r.title, value);}
        }
        public string author {
            get { return this.Retrieve(r => r.author); }
            set { this.Store(r => r.author, value); }
        }
        public string price {
            get { return this.Retrieve(r => r.price); }
            set { this.Store(r => r.price, value); }
        }
        public string price_suffix_apple {
            get { return this.Retrieve(r => r.price_suffix_apple); }
            set { this.Store(r => r.price_suffix_apple, value); }
        }
        public string price_suffix_google {
            get { return this.Retrieve(r => r.price_suffix_google); }
            set { this.Store(r => r.price_suffix_google, value); }
        }
        public string icon_apple {
            get { return this.Retrieve(r => r.icon_apple); }
            set { this.Store(r => r.icon_apple, value); }
        }
        public string icon_google {
            get { return this.Retrieve(r => r.icon_google); }
            set { this.Store(r => r.icon_google, value); }
        }
        public string button {
            get { return this.Retrieve(r => r.button); }
            set { this.Store(r => r.button, value); }
        }
        public string button_url_apple {
            get { return this.Retrieve(r => r.button_url_apple); }
            set { this.Store(r => r.button_url_apple, value); }
        }
        public string button_url_google {
            get { return this.Retrieve(r => r.button_url_google); }
            set { this.Store(r => r.button_url_google, value); }
        }
        public string enabled_platforms {
            get { return this.Retrieve(r => r.enabled_platforms); }
            set { this.Store(r => r.enabled_platforms, value); }
        }
        public string exclude_user_agent_regex {
            get { return this.Retrieve(r => r.exclude_user_agent_regex); }
            set { this.Store(r => r.exclude_user_agent_regex, value); }
        }
        public string include_user_agent_regex {
            get { return this.Retrieve(r => r.include_user_agent_regex); }
            set { this.Store(r => r.include_user_agent_regex, value); }
        }
        public string disable_positioning {
            get { return this.Retrieve(r => r.disable_positioning); }
            set { this.Store(r => r.disable_positioning, value); }
        }
        public string hide_ttl {
            get { return this.Retrieve(r => r.hide_ttl); }
            set { this.Store(r => r.hide_ttl, value); }
        }
        public string hide_path {
            get { return this.Retrieve(r => r.hide_path); }
            set { this.Store(r => r.hide_path, value); }
        }
        public string custom_design_modifier {
            get { return this.Retrieve(r => r.custom_design_modifier); }
            set { this.Store(r => r.custom_design_modifier, value); }
        }
    }

    [OrchardFeature("Laser.Orchard.BannerAgent")]
    public class BannerAgentPartRecord : ContentPartRecord {
        //https://github.com/ain/smartbanner.js
        public BannerAgentPartRecord() {
            title = "";
            author = "Laser SRL";
            price = "FREE";
            price_suffix_apple = " - On the App Store";
            price_suffix_google = " - In Google Play";
            icon_apple = "https://url/to/apple-store-icon.png";
            icon_google = "https://url/to/google-play-icon.png";
            button = "VIEW";
            button_url_apple = "https://ios/application-url";
            button_url_google = "https://android/application-url";
            enabled_platforms = "android,ios";
        }
        public virtual string title { get; set; }
        public virtual string author { get; set; }
        public virtual string price { get; set; }
        public virtual string price_suffix_apple { get; set; }
        public virtual string price_suffix_google { get; set; }
        public virtual string icon_apple { get; set; }
        public virtual string icon_google { get; set; }
        public virtual string button { get; set; }
        public virtual string button_url_apple { get; set; }
        public virtual string button_url_google   { get; set; }
        public virtual string enabled_platforms { get; set; }

        public virtual string exclude_user_agent_regex { get; set; }
        public virtual string include_user_agent_regex { get; set; }
        public virtual string disable_positioning { get; set; }
        public virtual string hide_ttl { get; set; }
        public virtual string hide_path { get; set; }
        public virtual string custom_design_modifier { get; set; }
    }
}