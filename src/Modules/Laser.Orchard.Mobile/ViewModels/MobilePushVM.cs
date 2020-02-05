using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.ViewModels {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushVM {
        public MobilePushVM() {
            PartId = 0;
            TitlePush = "";
            TextPush = "";
            ToPush = false;
            TestPush = false;
            DevicePush = "All";
            PushSent = false;
            TargetDeviceNumber = 0;
            PushSentNumber = 0;
            PushAdvertising = true;
            PushTestNumber = 0;
            UseRecipientList = false;
            TestPushToDevice = false;
        }
        public int PartId { get; set; }
        public string TitlePush { get; set; }
        public string TextPush { get; set; }
        public bool ToPush { get; set; }
        public bool TestPush { get; set; }
        public bool TestPushToDevice { get; set; }
        public string DevicePush { get; set; }
        public bool UseRecipientList { get; set; }
        public string RecipientList { get; set; }


        // proprietà aggiuntive
        public bool PushSent { get; set; }
        public int TargetDeviceNumber { get; set; }
        public int PushSentNumber { get; set; }
        public SelectList ListOfDevice { get; set; }
        public bool ShowTestOptions { get; set; }
        public bool HideRelated { get; set; }
        public bool PushAdvertising { get; set; }
        public int PushTestNumber { get; set; }
        public string SiteUrl { get; set; }
        public NotificationsCounters SentCounters { get; set; }
    }
}