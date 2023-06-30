using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires {
    public enum QuestionType {
        SingleChoice, MultiChoice, OpenAnswer
    };
    public enum AnswerType {
       None, Email, Url , Date , Datetime, Number
    };
    public enum ConditionType {
        Show,Hide
    };
    public enum ValidationAnswerType {
        AcceptTerms, RequiredAnswer, ImpossibleValidation, InvalidEmailAddress, InvalidInternetAddress, InvalidDate, InvalidDateTime, InvalidNumber
    }
}