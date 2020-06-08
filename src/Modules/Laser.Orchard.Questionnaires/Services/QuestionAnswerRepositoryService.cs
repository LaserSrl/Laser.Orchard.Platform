using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Questionnaires.Models;
using Orchard.Data;

namespace Laser.Orchard.Questionnaires.Services {
    public class QuestionAnswerRepositoryService : IQuestionAnswerRepositoryService {

        private readonly IRepository<QuestionRecord> _repositoryQuestions;
        private readonly IRepository<AnswerRecord> _repositoryAnswer;

        public QuestionAnswerRepositoryService(IRepository<QuestionRecord> repositoryQuestions, IRepository<AnswerRecord> repositoryAnswer) {
            _repositoryQuestions = repositoryQuestions;
            _repositoryAnswer = repositoryAnswer;
        }

        public IRepository<AnswerRecord> AnswersRepository() {
            return _repositoryAnswer;
        }
        public IRepository<QuestionRecord> QuestionsRepository() {
            return _repositoryQuestions;
        }
        public void DeleteAnswer(int id) {
            _repositoryAnswer.Delete(_repositoryAnswer.Get(id));
        }

        public void DeleteQuestion(int id) {
            _repositoryQuestions.Delete(_repositoryQuestions.Get(id));
        }

        public void FlushAnswer() {
            _repositoryAnswer.Flush();
        }

        public void FlushQuestion() {
            _repositoryQuestions.Flush();
        }

        public void UpdateAnswer(AnswerRecord record) {
            if (string.IsNullOrWhiteSpace(record.GUIdentifier)) {
                record.GUIdentifier = Guid.NewGuid().ToString("n");
            }
            if (record.Id == 0) {
                _repositoryAnswer.Create(record);
            }
            else {
                _repositoryAnswer.Update(record);
            }
        }

        public void UpdateQuestion(QuestionRecord record) {
            if (string.IsNullOrWhiteSpace(record.GUIdentifier)) {
                record.GUIdentifier = Guid.NewGuid().ToString("n");
            }
            if (record.Id == 0) {
                _repositoryQuestions.Create(record);
            }
            else {
                _repositoryQuestions.Update(record);
            }
        }

    }
}