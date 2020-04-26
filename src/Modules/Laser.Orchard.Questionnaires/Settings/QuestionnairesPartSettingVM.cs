using System;

namespace Laser.Orchard.Questionnaires.Settings {

    public class QuestionnairesPartSettingVM {

        public Int32 QuestionsLimitsNumber { get; set; }

        public Int32 QuestionsSortedRandomlyNumber { get; set; }

        public Int32 QuestionsResponseLimitsNumber { get; set; }

        public bool ShowCorrectResponseFlag { get; set; }

        public bool EnableQuestionImage { get; set; }

        public bool EnableAnswerImage { get; set; }

        public Int32 QuestionImageLimitNumber { get; set; }

        public Int32 AnswerImageLimitNumber { get; set; }

        public bool RandomResponse { get; set; }

        public bool AllowSections { get; set; }
        
        public bool AllowConditions { get; set; }

        public bool AllowSingleChoice { get; set; }
        public bool AllowMultiChoice { get; set; }
        public bool AllowOpenAnswers { get; set; }
        public string QuestionnaireContext { get; set; }
        public bool ForceAnonymous { get; set; }
        public bool ShowLatestAnswers { get; set; }

        public QuestionnairesPartSettingVM() {
            this.QuestionsLimitsNumber = 0;
            this.QuestionsSortedRandomlyNumber = 0;
            this.QuestionsResponseLimitsNumber = 0;
            this.ShowCorrectResponseFlag = false;
            this.RandomResponse = false;
            this.EnableQuestionImage = false;
            this.EnableAnswerImage = false;
            this.QuestionsResponseLimitsNumber = 0;
            this.AnswerImageLimitNumber = 0;
            this.AllowSections = true;
            this.AllowConditions = true;
            this.AllowSingleChoice = true;
            this.AllowMultiChoice  = true;
            this.AllowOpenAnswers = true;
            this.QuestionnaireContext = "";
            ForceAnonymous = false;
            ShowLatestAnswers = false;
        }
    }
}