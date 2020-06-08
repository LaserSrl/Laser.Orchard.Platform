using Orchard.Mvc;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class UserEventHandler:IUserEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkflowManager _workflowManager;

        public UserEventHandler(IWorkflowManager workflowManager, IHttpContextAccessor httpContextAccessor) {
            _workflowManager = workflowManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public void AccessDenied(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "AccessDenied" } });

        }

        public void Approved(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "Approved" } });
         
        }

        public void Moderate(global::Orchard.Security.IUser user){
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "Disabled" } });

        }

        public void ChangedPassword(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "ChangedPassword" } });
        }

        public void ConfirmedEmail(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "ConfirmedEmail" } });

        }

        public void Created(UserContext context) {
            var content = context.User.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", 
                content, 
                () => new Dictionary<string, object> {
                    { "Content", content },
                    { "Action", "Created" },
                    { "UserContext", context}
                });

        }

        public void Creating(UserContext context) {
            var content = context.User.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", 
                content, 
                () => new Dictionary<string, object> {
                    { "Content", content },
                    { "Action", "Creating" },
                    { "UserContext", context }
                });

        }

        public void LoggedIn(global::Orchard.Security.IUser user) {
            var Email = _httpContextAccessor.Current().Request.QueryString["Email"];
            if (!string.IsNullOrWhiteSpace(Email)) {
                ((UserPart)user).Email = Email;
            }
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "LoggedIn" } });

        }
               
      

        public void LoggedOut(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "LoggedOut" } });

        }

        public void LoggingIn(string userNameOrEmail, string password) {
            //TODO: Workflowing event. makes sense?
        }

        public void LogInFailed(string userNameOrEmail, string password) {
            //TODO: Workflowing event. makes sense?
        }

        public void SentChallengeEmail(global::Orchard.Security.IUser user) {
            var content = user.ContentItem;
            _workflowManager.TriggerEvent("OnUserEvent", content, () => new Dictionary<string, object> { { "Content", content }, { "Action", "SentChallengeEmail" } });

        }

    }
}