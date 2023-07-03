using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Questionnaires.Services {
    public interface IQuestionnaireHelperServices : IDependency {
        string GetValidationString(ValidationAnswerType answerType);
    }
}
