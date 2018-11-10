using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using OProjections = Orchard.Projections;
using Orchard;
using Orchard.Data;
using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;

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
            // recupera il context da un campo tokenized
            var questionnaireContext = (string)(context.State.Context);
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                List<Int32> elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.User_Id == currentUser.Id && x.Context == questionnaireContext).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
                if (elencoIdQuestionnaires.Count == 0) {
                    elencoIdQuestionnaires.Add(0);
                }
                foreach (var id in elencoIdQuestionnaires) {
                    var termId = id;
                    Action<IAliasFactory> selector =
                        alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                    Action<IHqlExpressionFactory> filter = x => x.NotEqProperty("Id", id.ToString());
                    context.Query.Where(selector, filter);
                }
            }
            else {
                // non estrae nulla: la condizione seguente è sempre falsa
                Action<IAliasFactory> selector =
                       alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter = x => x.EqProperty("Id", "-100");
                context.Query.Where(selector, filter);
            }
        }
    }
}