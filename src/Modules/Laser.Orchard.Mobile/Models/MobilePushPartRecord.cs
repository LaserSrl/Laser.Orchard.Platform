using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushPart : ContentPart<MobilePushPartRecord> {
        public string TitlePush {
            get { return Record.TitlePush; }
            set { Record.TitlePush = value; }
        }
        public string TextPush {
            get { return Record.TextPush; }
            set { Record.TextPush = value; }
        }
        public bool ToPush {
            get { return Record.ToPush; }
            set { Record.ToPush = value; }
        }
        public bool TestPush {
            get { return Record.TestPush; }
            set { Record.TestPush = value; }
        }

        public bool TestPushToDevice {
            get { return Record.TestPushToDevice; }
            set { Record.TestPushToDevice = value; }
        }

        public string DevicePush {
            get { return Record.DevicePush; }
            set { Record.DevicePush = value; }
        }

        public bool UseRecipientList {
            get { return Record.UseRecipientList; }
            set { Record.UseRecipientList = value; }
        }
        public string RecipientList {
            get { return Record.RecipientList; }
            set { Record.RecipientList = value; }
        }
        // proprietà aggiuntive
        public bool PushSent {
            get { return Record.PushSent; }
            set { Record.PushSent = value; }
        }
        public int TargetDeviceNumber {
            get { return Record.TargetDeviceNumber; }
            set { Record.TargetDeviceNumber = value; }
        }
        public int PushSentNumber {
            get { return Record.PushSentNumber; }
            set { Record.PushSentNumber = value; }
        }
    }

    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushPartRecord : ContentPartRecord {
        public virtual string TitlePush { get; set; }
        public virtual string TextPush { get; set; }
        public virtual bool ToPush { get; set; }
        public virtual bool TestPush { get; set; }
        public virtual bool TestPushToDevice { get; set; }
        public virtual string DevicePush { get; set; }
        public virtual bool UseRecipientList { get; set; }
        public virtual string RecipientList { get; set; }
        // proprietà aggiuntive
        public virtual bool PushSent { get; set; }
        public virtual int TargetDeviceNumber { get; set; }
        public virtual int PushSentNumber { get; set; }

    }
}
