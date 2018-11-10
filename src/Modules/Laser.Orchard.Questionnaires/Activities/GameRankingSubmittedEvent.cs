using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;


namespace Laser.Orchard.Questionnaires.Activities {
    public class GameRankingSubmittedgEvent : Event {
        public Localizer T { get; set; }

        public override bool CanStartWorkflow {
            get { return true; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override LocalizedString Category {
            get { return T("Questionnaire"); }
        }
        public override string Name {
            get { return "GameRankingSubmitted"; }
        }

        public override LocalizedString Description {
            get { return T("Questionnaire - Game Ranking Submitted)."); }
        }
    }
}
