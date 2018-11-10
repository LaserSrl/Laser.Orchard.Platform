using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Laser.Orchard.Twitter.Models {

    public class TwitterPostPart : ContentPart<TwitterPostPartRecord> {

        [DisplayName("Twitter")]
        public string TwitterMessage {
            get { return this.Retrieve(r => r.TwitterMessage); }
            set { this.Store(r => r.TwitterMessage, value); }
        }

        public bool TwitterMessageSent {
            get { return this.Retrieve(r => r.TwitterMessageSent); }
            set { this.Store(r => r.TwitterMessageSent, value); }
        }

        #region card

        public string TwitterTitle {
            get { return this.Retrieve(r => r.TwitterTitle); }
            set { this.Store(r => r.TwitterTitle, value); }
        }

        public string TwitterDescription {
            get { return this.Retrieve(r => r.TwitterDescription); }
            set { this.Store(r => r.TwitterDescription, value); }
        }

        public string TwitterPicture {
            get { return this.Retrieve(r => r.TwitterPicture); }
            set { this.Store(r => r.TwitterPicture, value); }
        }

        #endregion card

        //public string TwitterName {
        //    get { return this.Retrieve(r => r.TwitterName); }
        //    set { this.Store(r => r.TwitterName, value); }
        //}
        public bool TwitterCurrentLink {
            get { return this.Retrieve(r => r.TwitterCurrentLink); }
            set { this.Store(r => r.TwitterCurrentLink, value); }
        }


        public string TwitterLink {
            get { return this.Retrieve(r => r.TwitterLink); }
            set { this.Store(r => r.TwitterLink, value); }
        }
        
        public bool SendOnNextPublish {
            get { return this.Retrieve(r => r.SendOnNextPublish); }
            set { this.Store(r => r.SendOnNextPublish, value); }
        }

        public Int32[] AccountList {
            get {
                try {
                    string tmp = this.Retrieve(r => r.AccountList);
                    return (tmp ?? "").Split(',').Select(Int32.Parse).ToArray();
                }
                catch (Exception) {
                    return new Int32[] { };
                }
            }
            set { this.Store(r => r.AccountList, string.Join(",", value)); }
        }
    }

    public class TwitterPostPartRecord : ContentPartRecord {

        [StringLengthMax]
        public virtual string TwitterMessage { get; set; }

        public virtual bool TwitterMessageSent { get; set; }
        public virtual string TwitterTitle { get; set; }

        [StringLengthMax]
        public virtual string TwitterDescription { get; set; }

        //public virtual string TwitterName { get; set; }
        public virtual string TwitterPicture { get; set; }

        public virtual string TwitterLink { get; set; }
        public virtual bool TwitterCurrentLink { get; set; }
        public virtual bool SendOnNextPublish { get; set; }

        public virtual string AccountList { get; set; }
    }
}