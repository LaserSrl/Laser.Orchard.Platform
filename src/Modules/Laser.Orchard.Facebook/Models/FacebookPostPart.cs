using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.ComponentModel;
using System.Linq;
public enum FacebookType { Post, ShareLink };
namespace Laser.Orchard.Facebook.Models {

    public class FacebookPostPart : ContentPart<FacebookPostPartRecord> {

        [DisplayName("Facebook")]
        public string FacebookMessage {
            get { return this.Retrieve(r => r.FacebookMessage); }
            set { this.Store(r => r.FacebookMessage, value); }
        }

        public bool FacebookMessageSent {
            get { return this.Retrieve(r => r.FacebookMessageSent); }
            set { this.Store(r => r.FacebookMessageSent, value); }
        }

        public string FacebookCaption {
            get { return this.Retrieve(r => r.FacebookCaption); }
            set { this.Store(r => r.FacebookCaption, value); }
        }

        public string FacebookDescription {
            get { return this.Retrieve(r => r.FacebookDescription); }
            set { this.Store(r => r.FacebookDescription, value); }
        }

        public string FacebookName {
            get { return this.Retrieve(r => r.FacebookName); }
            set { this.Store(r => r.FacebookName, value); }
        }

        public string FacebookPicture {
            get { return this.Retrieve(r => r.FacebookPicture); }
            set { this.Store(r => r.FacebookPicture, value); }
        }
        public string FacebookIdPicture {
            get { return this.Retrieve(r => r.FacebookIdPicture); }
            set { this.Store(r => r.FacebookIdPicture, value); }
        }

        public string FacebookLink {
            get { return this.Retrieve(r => r.FacebookLink); }
            set { this.Store(r => r.FacebookLink, value); }
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

        public FacebookType FacebookType {
            get { return this.Retrieve(r => r.FacebookType); }
            set { this.Store(r => r.FacebookType, value); }
        }
        
        public string FacebookMessageToPost {
            get { return this.Retrieve(r => r.FacebookMessageToPost); }
            set { this.Store(r => r.FacebookMessageToPost, value); }
        }
        public bool HasImage {
            get { return this.Retrieve(r => r.HasImage); }
            set { this.Store(r => r.HasImage, value); }
        }
        
    }

    public class FacebookPostPartRecord : ContentPartRecord {

        [StringLengthMax]
        public virtual string FacebookMessage { get; set; }

        public virtual bool FacebookMessageSent { get; set; }
        public virtual string FacebookCaption { get; set; }

        [StringLengthMax]
        public virtual string FacebookDescription { get; set; }

        public virtual string FacebookName { get; set; }
        public virtual string FacebookPicture { get; set; }
        public virtual string FacebookIdPicture { get; set; }
        public virtual string FacebookLink { get; set; }
        public virtual string AccountList { get; set; }
        public virtual bool SendOnNextPublish { get; set; }
        public virtual FacebookType FacebookType { get; set; }
        public virtual string FacebookMessageToPost { get; set; }
        public virtual bool HasImage { get; set; }

    }
}