using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Newtonsoft.Json;

namespace Laser.Orchard.StartupConfig.Activities {
    public class WebApiResponseActivity : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IActivityServices _activityServices;
        private readonly ICommonsServices _commonServices;
        public WebApiResponseActivity(IOrchardServices orchardServices,
            IActivityServices activityServices, ICommonsServices commonServices) {
            _orchardServices = orchardServices;
            _activityServices = activityServices;
            _commonServices = commonServices;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public override string Form {
            get {
                return "WebApiResponseEditForm";
            }
        }

        public override LocalizedString Category {
            get { return T("Utils"); }
        }

        public override string Name {
            get { return "WebApiResponse"; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override LocalizedString Description {
            get { return T("When called from a WebApi Signal Trigger, it returns a Response object."); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {

            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var tokenWebApiResponseName = "WebApiResponse";
            if (workflowContext.Tokens.ContainsKey(tokenWebApiResponseName)  && workflowContext.Tokens[tokenWebApiResponseName] != null) {
                bool success = activityContext.GetState<bool>("Successful");
                string message = activityContext.GetState<string>("Message");
                var dataString = activityContext.GetState<string>("Data");
                if (!string.IsNullOrWhiteSpace(dataString)) {
                    try {
                        ((Response)workflowContext.Tokens[tokenWebApiResponseName]).Data =
                            JsonConvert.DeserializeObject(dataString);
                    }
                    catch { }
                }
                ErrorCode errorCode = activityContext.GetState<ErrorCode>("ErrorCode");
                ResolutionAction resolutionAction = activityContext.GetState<ResolutionAction>("ResolutionAction");
                ((Response)workflowContext.Tokens[tokenWebApiResponseName]).Success = success;
                ((Response)workflowContext.Tokens[tokenWebApiResponseName]).Message = message;
                if (!success) {
                    ((Response)workflowContext.Tokens[tokenWebApiResponseName]).ErrorCode = errorCode;
                    ((Response)workflowContext.Tokens[tokenWebApiResponseName]).ResolutionAction = resolutionAction;
                } else {
                    ((Response)workflowContext.Tokens[tokenWebApiResponseName]).ErrorCode = ErrorCode.NoError;
                    ((Response)workflowContext.Tokens[tokenWebApiResponseName]).ResolutionAction = ResolutionAction.NoAction;
                }
            }
            yield return T("Done");
        }
    }
}