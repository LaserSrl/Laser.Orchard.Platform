using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard;
using Laser.Orchard.ButtonToWorkflows.Models;

namespace Laser.Orchard.ButtonToWorkflows.Activity {
    public class ButtonToWorkflowsEvent : Event {
        protected readonly IOrchardServices _orchardServices;
        public ButtonToWorkflowsEvent(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public Localizer T { get; set; }
        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            try {

                var contentTypesState = activityContext.GetState<string>("ContentTypes");

                // "" means 'any'
                if (String.IsNullOrEmpty(contentTypesState)) {
                    return true;
                }

                string[] contentTypes = contentTypesState.Split(',');

                var content = workflowContext.Content;

                if (content == null) {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
            }
            catch {
                return false;
            }
        }
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {


            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Content Event"); }
        }
        public override string Name {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsAction.Split('£');
                    return v[0]+"_btn1";
                }
                catch { return "CustomButton1"; }
            }
        }
        public override string Form {
            get { return "_SelectContentTypes"; }
        }

        public override LocalizedString Description {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsDescription.Split('£');
                    return T(v[0]);
                }
                catch { return T("Custom Button"); }
            }

        }

    }

    public class CustomButton2 : ButtonToWorkflowsEvent {
        
        public CustomButton2(IOrchardServices orchardServices)
            : base(orchardServices) {
        }

        public override string Name {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsAction.Split('£');
                    return v[1]+"_btn2";
                }
                catch { return "CustomButton2"; }
            }
        }
        public override LocalizedString Description {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsDescription.Split('£');
                    return T(v[1]);
                }
                catch { return T("Custom Button"); }
            }

        }


    }
    public class CustomButton3 : ButtonToWorkflowsEvent {

        public CustomButton3(IOrchardServices orchardServices)
            : base(orchardServices) {
        }
        public override string Name {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsAction.Split('£');
                    return v[2]+"_btn3";
                }
                catch { return "CustomButton3"; }
            }
        }

        public override LocalizedString Description {
            get {
                try {
                    string[] v = ((ButtonToWorkflowsSettingsPart)(((dynamic)_orchardServices.WorkContext.CurrentSite).ButtonToWorkflowsSettingsPart)).ButtonsDescription.Split('£');
                    return T(v[2]);
                }
                catch { return T("Custom Button"); }
            }

        }

    }
}

