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
using System.Reflection;
using System.Web;

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
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                List<Int32> elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.User_Id == currentUser.Id).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
                //smarco tutte le lingue
                //foreach (Int32 idquestion in elencoIdQuestionnaires) {
                //    _orchardServices.ContentManager.Query<QuestionnairePart,QuestionnairePartRecord>().Where(x=>x.Id==
                //}
                IHqlQuery query = context.Query;
                query.Join(alias => alias.ContentPartRecord(typeof(QuestionnairePartRecord)));
                foreach (var id in elencoIdQuestionnaires) {
                    var termId = id;
                    Action<IAliasFactory> selector =
                        alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                    Action<IHqlExpressionFactory> filter = x => x.NotEqProperty("Id", id.ToString());
                    context.Query.Where(selector, filter);
                }
            }
            else {
                IHqlQuery query = context.Query;
                query.Join(alias => alias.ContentPartRecord(typeof(QuestionnairePartRecord)));
                Action<IAliasFactory> selector =
                       alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter = x => x.EqProperty("Id", "-100");
                context.Query.Where(selector, filter);
            }
            //var defaultHqlQuery = query as DefaultHqlQuery;
            //query = query.ForType("Questionnarie").ForVersion(VersionOptions.Published);
           
            //var defaultHqlQuery = query as DefaultHqlQuery;
            //var fiJoins = typeof(DefaultHqlQuery).GetField("_joins", BindingFlags.Instance | BindingFlags.NonPublic);
            //var joins = fiJoins.GetValue(defaultHqlQuery) as List<Tuple<IAlias, Join>>;
            //joins.Add(new Tuple<IAlias, Join>(new Alias("Laser.Orchard.Questionnaires.Models"), new Join("UserAnswersRecord", "UserAnswers", ",")));



            //query.Join(alias => alias.ContentPartRecord(typeof(QuestionnairePartRecord)));
            //context.Query = query.Where(
            //alias => alias.Named("UserAnswers"),
            //    //   g=>g.Disjunction(
            //    f => f.Conjunction(
            //    x => x.EqProperty("QuestionnairePartRecord_Id", "questionnairePartRecord.Id"),
            //    predicate => predicate.NotEqProperty("User_Id", currentUser.Id.ToString())
            //    )
            //    //,
            //    // x=>x.

            //    );


           
            //context.Query = query.Where(
            //alias => alias.Named("UserAnswers"),

            //    g => g.Conjunction(
            //        h => h.EqProperty("QuestionnairePartRecord_Id", "questionnairePartRecord.Id"),
            //        i => i.Not(
            //    f => f.Conjunction(
            //    x => x.EqProperty("QuestionnairePartRecord_Id", "questionnairePartRecord.Id"),
            //    predicate => predicate.EqProperty("User_Id", currentUser.Id.ToString())
            //    )
            //    )
            //        )

            //    );

            //context.Query = query.Where(y => y.ContentPartRecord<QuestionnairePartRecord>(), x => x.NotEqProperty("Id", currentUser.Id.ToString()));








            //       Action<IAliasFactory> s = alias => alias.ContentPartRecord<QuestionnairePartRecord>();
            //       Action<IHqlExpressionFactory> f = y => y.NotEqProperty("UserPartRecord.Id", currentUser.Id.ToString());
            //       context.Query.Where(s, f);

            // query.Join(alias => alias.ContentPartRecord("UserPartRecord"));
            //  query.Join(alias=>alias.
            //    QuestionnairePartRecord_Id            
            // context.Query = context.Query.Join(x => x.ContentPartRecord(typeof(QuestionnairePartRecord)));

            //  context.Query = query.Where(x => x.ContentPartRecord<UserPartRecord>(),x=>x.Ge("Id",0));ù

            //   query.Join(alias => alias.ContentItem());

            // var defaultHqlQuery = query as DefaultHqlQuery;
            //        context.Query = query.ForType("Push");

            //     context.Query = context.Query.Join(x => x.ContentPartRecord(typeof(UserDeviceRecord)));

            //  context.Query = query.Where(x => x.ContentPartRecord<UserPartRecord>(),x=>x.Ge("Id",0));ù
            //    query.ForPart<UserPart>().ForVersion(VersionOptions.Latest);

            //context.Query = context.Query.Join(x => x.ContentPartRecord(typeof(UserPartRecord)));
            //context.Query = context.Query.Where(y => y.ContentPartRecord(typeof(UserPartRecord)), y => y.Ge("Id", 0));


            //  context.Query = query.Where(x => x.ContentItem(), x => x.Eq("Id", HardCodedId));

            //  query.Join(alias => alias.ContentPartRecord());

            //  Action<IHqlExpressionFactory> joinOn = predicate => predicate.EqProperty("ContentItemRecord", "ci");
            //query = query.Where(
            //alias => alias.Named("UserDevice"),
            //joinOn
            //);

            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Only unanswered Questionnare for current user");
        }

    }
}

