using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Activities {
    public class UserEventActivity  : Event {
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;
        public UserEventActivity(IContentManager contentManager) {
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
           
        }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override string Name {
            get {
                return "OnUserEvent";
                //    throw new NotImplementedException(); 
            }
        }

        public override LocalizedString Category {
            get {
                return T("Users");// throw new NotImplementedException();
            }
        }

        public override LocalizedString Description {
            get {
                return T("Manage User Event");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Created"),
                T("AccessDenied"),
                T("Approved"),
                T("Disabled"),
                T("ChangedPassword"),
                T("ConfirmedEmail"),
                T("LoggedIn"),
                T("LoggedOut"),
                T("SentChallengeEmail"),
                T("Creating")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string operatore = workflowContext.Tokens["Action"].ToString();
            LocalizedString messageout =T(operatore);
            yield return messageout;
        }
    }
}


