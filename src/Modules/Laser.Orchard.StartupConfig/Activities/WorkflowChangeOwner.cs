using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Workflows.Services;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Users.Models;



namespace Laser.Orchard.StartupConfig.Activities {
    public class WorkflowChangeOwnerActivity : Task {
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;
        public WorkflowChangeOwnerActivity(IContentManager contentManager) {
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }



        public override string Name {
            get {
                return "ChangeUserOwner";
                //    throw new NotImplementedException(); 
            }
        }

        public override string Form {
            get {
                return "TheFormActivityChangeOwner";
            }
        }

        public override LocalizedString Category {
            get {
                return T("Content");// throw new NotImplementedException();
            }
        }

        public override LocalizedString Description {
            get {
                return T("Change User Owner of content item");
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Success"), T("Error") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            LocalizedString messageout = T("Success");
            try {
                var newowner = activityContext.GetState<string>("allusers");
                if (String.IsNullOrEmpty(newowner)) {
                    messageout = T("Error");
                }
                var content = workflowContext.Content;

                UserPart userpart = _contentManager
               .Query<UserPart, UserPartRecord>()
               .Where(x => x.Id == Convert.ToInt32(newowner)).List().FirstOrDefault();



                ((dynamic)content.ContentItem).CommonPart.Owner = userpart;
            }
            catch { messageout = T("Error"); }
            yield return messageout;
        }
    }
}


