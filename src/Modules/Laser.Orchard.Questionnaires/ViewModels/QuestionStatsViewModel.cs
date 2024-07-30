using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionStatsViewModel {
        public int QuestionnairePart_Id { get; set; }
        public string QuestionnaireTitle { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public int Position { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<AnswerStatsViewModel> Answers { get; set; }
        public QuestionStatsViewModel() {
            Answers = new List<AnswerStatsViewModel>();
        }
    }
}