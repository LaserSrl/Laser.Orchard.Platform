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
        QuestionnaireStatsViewModel GetStats(int questionnaireId, DateTime? from = null, DateTime? to = null);
        List<QuestStatViewModel> GetStats(QuestionType type);
        bool Save(QuestionnaireWithResultsViewModel editModel, IUser currentUser, string SessionID);
        /// <summary>
        /// Extract the most recent set of answers that a specific user has given
        /// to a specific questionnaire.
        /// </summary>
        /// <param name="part">The QuestionnairePart representing the Questionnaire whose
        /// answers we want to extract.</param>
        /// <param name="user">The IUser representing the user whose answers we want to
        /// extract</param>
        /// <returns>A view model containing the set of answers given.</returns>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if
        /// either parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if
        /// the latest answers given by the user for the question are older than the
        /// introduction of answers' instances.</exception>
        QuestionnaireWithResultsViewModel GetMostRecentAnswersInstance(
            QuestionnairePart part, IUser user);
        bool SendTemplatedEmailRanking();
        bool SendTemplatedEmailRanking(Int32 gameID);
        void ScheduleEmailTask(Int32 gameID, DateTime timeGameEnd);
        void UnscheduleEmailTask(Int32 gameID);
        List<RankingTemplateVM> QueryForRanking(
           Int32 gameId, string device = "General", int page = 1, int pageSize = 10, bool Ascending = false);
        void SaveQuestionnaireUsersAnswers(int questionnaireId, DateTime? from = null, DateTime? to = null);
    }
}
