using Laser.Orchard.Questionnaires.Models;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Services {
    public interface IQuestionAnswerRepositoryService : IDependency {
        IRepository<AnswerRecord> AnswersRepository();
        IRepository<QuestionRecord> QuestionsRepository();
        void UpdateAnswer(AnswerRecord record);
        void UpdateQuestion(QuestionRecord record);
        void DeleteAnswer(int id);
        void DeleteQuestion(int id);
        void FlushAnswer();
        void FlushQuestion();
    }
}


