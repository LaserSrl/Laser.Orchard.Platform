using System.Collections.Generic;

namespace Laser.Orchard.Translator.ViewModels
{
    public class TranslationDetailViewModel
    {
        public string containerName;
        public string containerType;
        public string language;
        public List<StringSummaryViewModel> messages;
    }

    public class StringSummaryViewModel
    {
        public int id;
        public string message;
        public bool localized;
    }
}