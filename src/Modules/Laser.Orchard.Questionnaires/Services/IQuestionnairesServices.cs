using System;
using System.Collections.Generic;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
namespace Laser.Orchard.Questionnaires.Services {
    public interface IQuestionnairesServices : IDependency {
        void UpdateForContentItem(ContentItem item, QuestionnaireEditModel partEditModel);
        QuestionnaireEditModel BuildEditModelForQuestionnairePart(QuestionnairePart part);
        QuestionnaireViewModel BuildViewModelForQuestionnairePart(QuestionnairePart part);
        QuestionnaireWithResultsViewModel BuildViewModelWithResultsForQuestionnairePart(QuestionnairePart part);
        void CreateUserAnswers(UserAnswersRecord answerRecord);
        AnswerRecord GetAnswer(int id);
        List<QuestionnaireStatsViewModel> GetStats(int questionnaireId, DateTime? from = null, DateTime? to = null);
        List<QuestStatViewModel> GetStats(QuestionType type);
        bool Save(QuestionnaireWithResultsViewModel editModel, IUser currentUser, string SessionID);
        bool SendTemplatedEmailRanking();
        bool SendTemplatedEmailRanking(Int32 gameID);
        void ScheduleEmailTask(Int32 gameID, DateTime timeGameEnd);
        void UnscheduleEmailTask(Int32 gameID);
        List<RankingTemplateVM> QueryForRanking(
           Int32 gameId, string device = "General", int page = 1, int pageSize = 10, bool Ascending = false);
        void SaveQuestionnaireUsersAnswers(int questionnaireId, DateTime? from = null, DateTime? to = null);
    }
}
