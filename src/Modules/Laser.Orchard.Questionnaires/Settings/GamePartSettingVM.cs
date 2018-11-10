using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.Settings {

    public class GamePartSettingVM {

        public bool SendEmail { get; set; }

        public string EmailRecipe { get; set; }

        public bool Ranking { get; set; }

        public int Template { get; set; }

        public List<TemplateLookup> ListTemplate { get; set; }

        public GamePartSettingVM() {
            this.SendEmail = true;
            this.Ranking = true;
        }
    }

    public class TemplateLookup {

        public int Id { get; set; }

        public string Name { get; set; }
    }
}