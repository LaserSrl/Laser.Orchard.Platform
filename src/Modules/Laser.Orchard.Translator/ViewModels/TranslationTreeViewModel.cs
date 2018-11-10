using System.Collections.Generic;

namespace Laser.Orchard.Translator.ViewModels
{
    public class TranslatorViewModel
    {
        public IEnumerable<string> CultureList { get; set; }
        public string selectedCulture;
        public string selectedFolderName;
        public string selectedFolderType;
    }
}