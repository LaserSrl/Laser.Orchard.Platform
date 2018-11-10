using System.Collections.Generic;

namespace Laser.Orchard.Translator.ViewModels
{
    public class TranslationTreeNodeViewModel
    {
        public string id;
        public string text;
        public string type;
        public Dictionary<string, string> data;
        public List<TranslationTreeNodeViewModel> children;
    }
}