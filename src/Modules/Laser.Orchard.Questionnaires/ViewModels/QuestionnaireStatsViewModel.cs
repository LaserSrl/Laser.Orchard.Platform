using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionnaireStatsViewModel {
        public QuestionnaireStatsViewModel() {
            QuestionsStatsList = new List<QuestionStatsViewModel>();
            FilterContext = new StatsDetailFilterContext();
        }
        public string Title { get; set; }
        public int Id { get; set; }
        public int ReplyingPeopleCount { get; set; }
        public List<QuestionStatsViewModel> QuestionsStatsList { get; set; }
        public int NumberOfQuestions { get; internal set; }
        public int FullyAnsweringPeople { get; internal set; }
        public StatsDetailFilterContext FilterContext { get; internal set; }
    }
}