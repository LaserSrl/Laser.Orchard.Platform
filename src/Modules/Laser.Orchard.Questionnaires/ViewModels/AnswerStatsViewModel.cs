using System;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class AnswerStatsViewModel {
        public string Answer { get; set; }
        public int Count { get; set; }
        public DateTime LastDate { get; set; }
    }
}