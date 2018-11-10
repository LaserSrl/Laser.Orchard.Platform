using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class AnswerWithResultViewModel : AnswerViewModel {
        public bool Answered { get; set; }

        [MaxLength(1200)]
        public string AnswerText { get; set; }
    }
}