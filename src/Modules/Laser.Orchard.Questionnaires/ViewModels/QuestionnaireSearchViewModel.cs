using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestionnaireSearchViewModel {
        public dynamic Pager { get; set; }
        public StatsSearchContext SearchContext { get; set; }
        public IEnumerable<ContentItem> Questionnaires { get; set; }
    }
}