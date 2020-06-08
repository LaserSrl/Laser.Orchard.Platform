using Laser.Orchard.Questionnaires.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using OProjections = Orchard.Projections;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class QuestionnaireNotAnsweredFilter : OProjections.Services.IFilterProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<UserAnswersRecord> _userAnswersRecord;
        public Localizer T { get; set; }

        public QuestionnaireNotAnsweredFilter(IOrchardServices orchardServices, IRepository<UserAnswersRecord> userAnswersRecord) {
            _orchardServices = orchardServices;
            _userAnswersRecord = userAnswersRecord;
            T = NullLocalizer.Instance;
        }
        public void Describe(OProjections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Questionnaire", T("Questionnaire"), T("Questionnaire"))
                .Element("Questionnaire Not Answered Filter", T("Specific questionnaire and context without response for current User"), T("Filter for specific questionnaire and context without response for current User."),
                    ApplyFilter,
                    DisplayFilter,
                    "QuestionnaireNotAnsweredFilterForm"
                );
        }
        public LocalizedString DisplayFilter(OProjections.Descriptors.Filter.FilterContext context) {
            return T("Only unanswered Questionnaire for current user");
        }
        public void ApplyFilter(OProjections.Descriptors.Filter.FilterContext context) {
            List<int> elencoIdQuestionnaires = new List<int>();

            // recupera il context da un campo tokenized
            var aux = (string)(context.State.QuestionnaireId);
            var questionnaireId = string.IsNullOrWhiteSpace(aux) ? 0 : Convert.ToInt32(aux);
            // recupera il questionnaire context
            var questionnaireContext = (string)(context.State.Context);

            string uniqueId = "";
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser != null) {
                elencoIdQuestionnaires = _userAnswersRecord
                    .Fetch(x => x.User_Id == currentUser.Id 
                        && x.QuestionnairePartRecord_Id == questionnaireId 
                        && x.Context == questionnaireContext)
                    .Select(y => y.QuestionnairePartRecord_Id)
                    .Distinct().ToList();
            } else {
                var request = _orchardServices.WorkContext.HttpContext.Request;

                if (request != null && request.Headers["x-uuid"] != null) {
                    uniqueId = request.Headers["x-uuid"];
                } else {
                    uniqueId = _orchardServices.WorkContext.HttpContext.Session.SessionID;
                }

                if (!string.IsNullOrWhiteSpace(uniqueId))
                    elencoIdQuestionnaires = _userAnswersRecord
                        .Fetch(x => x.SessionID == uniqueId 
                            && x.QuestionnairePartRecord_Id == questionnaireId 
                            && x.Context == questionnaireContext)
                        .Select(y => y.QuestionnairePartRecord_Id)
                        .Distinct().ToList();
            }

            if (currentUser != null || !string.IsNullOrWhiteSpace(uniqueId)) {
                Action<IAliasFactory> selector =
                    alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter1 = x => x.Eq("Id", questionnaireId);
                context.Query.Where(selector, filter1);
                foreach (var id in elencoIdQuestionnaires) {
                    Action<IHqlExpressionFactory> filter2 = x => x.NotEqProperty("Id", id.ToString());
                    context.Query.Where(selector, filter2);
                }
            }
        }
    }
}