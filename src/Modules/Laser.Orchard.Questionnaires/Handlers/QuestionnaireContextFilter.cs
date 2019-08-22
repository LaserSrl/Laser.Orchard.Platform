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
    public class QuestionnaireContextFilter : OProjections.Services.IFilterProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<UserAnswersRecord> _userAnswersRecord;
        public Localizer T { get; set; }

        public QuestionnaireContextFilter(IOrchardServices orchardServices, IRepository<UserAnswersRecord> userAnswersRecord) {
            _orchardServices = orchardServices;
            _userAnswersRecord = userAnswersRecord;
            T = NullLocalizer.Instance;
        }
        public void Describe(OProjections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Questionnaire", T("Questionnaire"), T("Questionnaire"))
                .Element("Questionnaire Context Filter", T("Questionnaires without response for current User on a specific context"), T("Filter for questionnaires without response for current User on a specific context."),
                    ApplyFilter,
                    DisplayFilter,
                    "QuestionnaireContextFilterForm"
                );
        }
        public LocalizedString DisplayFilter(OProjections.Descriptors.Filter.FilterContext context) {
            return T("Only unanswered Questionnare for current user on a specific context");
        }
        public void ApplyFilter(OProjections.Descriptors.Filter.FilterContext context) {
            List<int> elencoIdQuestionnaires = new List<int>();

            // recupera il context da un campo tokenized
            var questionnaireContext = (string)(context.State.Context);
            var currentUser = _orchardServices.WorkContext.CurrentUser;

            if (currentUser != null) {
                elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.User_Id == currentUser.Id && x.Context == questionnaireContext).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
            }
            else {
                string uniqueId;
                var request = _orchardServices.WorkContext.HttpContext.Request;

                if (request != null && request.Headers["x-uuid"] != null) {
                    uniqueId = request.Headers["x-uuid"];
                } else {
                    uniqueId = _orchardServices.WorkContext.HttpContext.Session.SessionID;
                }

                if (!string.IsNullOrWhiteSpace(uniqueId))
                    elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.SessionID == uniqueId && x.Context == questionnaireContext).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
            }

            foreach (var id in elencoIdQuestionnaires) {
                Action<IAliasFactory> selector =
                    alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter = x => x.NotEqProperty("Id", id.ToString());
                context.Query.Where(selector, filter);
            }
        }
    }
}