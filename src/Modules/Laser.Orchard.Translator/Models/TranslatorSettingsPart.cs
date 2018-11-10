using Orchard.ContentManagement;

namespace Laser.Orchard.Translator.Models
{
    public class TranslatorSettingsPart : ContentPart
    {
        public string ModulesToTranslate
        {
            get { return this.Retrieve(x => x.ModulesToTranslate); }
            set { this.Store(x => x.ModulesToTranslate, value); }
        }

        public string ThemesToTranslate
        {
            get { return this.Retrieve(x => x.ThemesToTranslate); }
            set { this.Store(x => x.ThemesToTranslate, value); }
        }
    }
}