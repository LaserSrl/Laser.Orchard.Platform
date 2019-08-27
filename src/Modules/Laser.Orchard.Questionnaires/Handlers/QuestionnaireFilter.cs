using Laser.Orchard.Questionnaires.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Events;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Handlers {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }
    public class QuestionnaireFilter : IFilterProvider {
        public Localizer T { get; set; }
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<UserAnswersRecord> _userAnswersRecord;

        public QuestionnaireFilter(IWorkContextAccessor workContextAccessor, IOrchardServices orchardServices, IRepository<UserAnswersRecord> userAnswersRecord) {
            T = NullLocalizer.Instance;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
            _orchardServices = orchardServices;
            _userAnswersRecord = userAnswersRecord;
        }

        public void Describe(dynamic describe) {
            describe
                    .For("Questionnaire", T("Questionnaire"), T("Questionnaire"))
                    .Element("Questionnaire", T("Questionnaire without response for current User"), T("Filter for Questionnaire with not associated response to current user"),
                    (Action<dynamic>)ApplyFilter,
                   (Func<dynamic, LocalizedString>)DisplayFilter,
                   "Questionnaire"
                             );

        }

        public void ApplyFilter(dynamic context) {
            List<int> elencoIdQuestionnaires = new List<int>();

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.User_Id == currentUser.Id).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
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
                    elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.SessionID == uniqueId).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
            }

            IHqlQuery query = context.Query;
            query.Join(alias => alias.ContentPartRecord(typeof(QuestionnairePartRecord)));
            foreach (var id in elencoIdQuestionnaires) {
                var termId = id;
                Action<IAliasFactory> selector =
                    alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter = x => x.NotEqProperty("Id", id.ToString());
                context.Query.Where(selector, filter);
            }

            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Only unanswered Questionnare for current user");
        }

    }
}

