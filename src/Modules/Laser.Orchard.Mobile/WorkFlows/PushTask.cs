using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Common.Fields;

namespace Laser.Orchard.Mobile.WorkFlows {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushTask : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecord;
        private readonly IScheduledTaskManager _taskManager;

        public ILogger Logger { get; set; }

        public PushTask(
            IOrchardServices orchardServices,
            IPushGatewayService pushGatewayService,
            IRepository<UserDeviceRecord> userDeviceRecord,
            IScheduledTaskManager taskManager
            ) {
            _pushGatewayService = pushGatewayService;
            _orchardServices = orchardServices;
            _userDeviceRecord = userDeviceRecord;
            _taskManager = taskManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Sent") };
        }

        public override string Form {
            get {
                return "ActivityMobileForm";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendPush"; }
        }

        public override LocalizedString Description {
            get { return T("Send Push."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            ContentItem contentItem = null;
            if (workflowContext != null && workflowContext.Content != null) 
                contentItem = workflowContext.Content.ContentItem;

            var device = activityContext.GetState<string>("allDevice");
            var PushMessage = activityContext.GetState<string>("PushMessage");
            bool produzione = activityContext.GetState<string>("Produzione") == "Produzione";
            var usersList = activityContext.GetState<string>("userId") ?? "";
            int iUser = 0;

            string users = "";
            string[] userList;
            if (device == "UserId") {
                int.TryParse(usersList, out iUser);
                userList = usersList.Split(',', ' ');
                List<int> iUserList = new List<int>();
                foreach (string uId in userList) {
                    if (int.TryParse(uId, out iUser)) {
                        iUserList.Add(iUser);
                    }
                }
                users = string.Join(",", iUserList);
            } else if (device.Equals("UserEmail")) {
                userList = usersList.Split(',', ' ');
                List<string> neUserList = new List<string>();
                foreach (string neUser in userList) {
                    neUserList.Add("'" + GetSafeSqlString(neUser) + "'");
                }
                users = string.Join(",", neUserList);
            } else if (device.Equals("UserName")) {
                userList = usersList.Split(',', ' ');
                List<string> neUserList = new List<string>();
                foreach (string neUser in userList) {
                    neUserList.Add("'" + GetSafeSqlString(neUser) + "'");
                }
                users = string.Join(",", neUserList);
            }
            
            int idRelated = 0;
            string stateIdRelated = activityContext.GetState<string>("idRelated");
            if (stateIdRelated == "idRelated" && contentItem != null) { //caso necessario per il pregresso
                idRelated = contentItem.Id;
            } else {
                int.TryParse(stateIdRelated, out idRelated);
            }
            string language = activityContext.GetState<string>("allLanguage");
            string querydevice = "";

            if (contentItem == null && (device == "ContentOwner" || device == "ContentCreator" || device == "ContentLastModifier")) {
                querydevice = " SELECT  distinct P.* " +
                                    " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                    " Where 0 = 1"; //necessario per non spedire le push a tutti
                device = "All";
            } else {
                if (device == "ContentOwner") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " Where U.UserPartRecord_Id=" + contentItem.As<CommonPart>().Owner.Id.ToString();
                    device = "All";
                }
                if (device == "ContentCreator") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " Where U.UserPartRecord_Id=" + ((dynamic)contentItem.As<CommonPart>()).Creator.Value.ToString();

                    device = "All";
                }
                if (device == "ContentLastModifier") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " Where U.UserPartRecord_Id=" + ((dynamic)contentItem.As<CommonPart>()).LastModifier.Value.ToString();

                    device = "All";
                }
                if (device == "UserId") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " Where U.UserPartRecord_Id in (" + users + ")";

                    device = "All";
                }
                if (device == "UserEmail") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " INNER JOIN Orchard_Users_UserPartRecord AS Ou ON Ou.Id = U.UserPartRecord_Id " +
                                        " Where Ou.Email in (" + users + ")";

                    device = "All";
                }
                if (device == "UserName") {
                    querydevice = " SELECT  distinct P.* " +
                                        " FROM  Laser_Orchard_Mobile_PushNotificationRecord AS P " +
                                        " LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS U ON P.UUIdentifier = U.UUIdentifier " +
                                        " INNER JOIN Orchard_Users_UserPartRecord AS Ou ON Ou.Id = U.UserPartRecord_Id " +
                                        " Where Ou.UserName in (" + users + ")";

                    device = "All";
                }
            }

            // schedule the sending of the push notification
            try {
                var ci = _orchardServices.ContentManager.Create("BackgroundPush");
                var part = ci.As<MobilePushPart>();
                part.DevicePush = device;
                part.PushSent = false;
                part.TestPush = !produzione;
                part.TestPushToDevice = false;
                part.TextPush = PushMessage;
                part.ToPush = true;
                part.UseRecipientList = false;
                var relatedContentField = part.Fields.FirstOrDefault(x => x.Name == "RelatedContent");
                if (relatedContentField != null) {
                    (relatedContentField as ContentPickerField).Ids = new int[] { idRelated };
                }
                // we cannot use part settings to specify this queryDevice value because it can change for each BackgroundPush item
                // so we use a dedicated field on this content item
                var queryField = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == "BackgroundPush").Fields.FirstOrDefault(x => x.Name == "QueryDevice");
                (queryField as TextField).Value = querydevice;
                // force publish of content item to manage the content picker field the right way (field index)
                // and to start scheduled task that will send push notifications
                ci.VersionRecord.Published = false;
                _orchardServices.ContentManager.Publish(ci);
            } catch (Exception ex) {
                Logger.Error(ex, "PushTask - Error starting asynchronous thread to send push notifications.");
            }
            yield return T("Sent");
        }

        private static IEnumerable<string> SplitEmail(string commaSeparated) {
            if (commaSeparated == null) return null;
            return commaSeparated.Split(new[] { ',', ';' });
        }
        // Converts a string in a safe string for SQL commands replacing single quotes(')
        private string GetSafeSqlString(string text) {
            return text.Replace("'", "''");
        }
    }
}