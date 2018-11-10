using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Pdf.Models {
    public class PdfButtonPartSettings {
        public IEnumerable<PdfButtonSettings> PdfButtons { get; set; }
        public PdfButtonPartSettings() {
            var defaultButton= new List<PdfButtonSettings>();
            defaultButton.Add(new PdfButtonSettings());
            PdfButtons = defaultButton;
        }
        public string ParseListToString() {
            var json = JToken.FromObject(PdfButtons);
            return json.ToString();
        }
        public void LoadStringToList(string toParse) {
            var list = new List<PdfButtonSettings>();
            var json = JToken.Parse(toParse);
            foreach(var el in json) {
                var button = el.ToObject<PdfButtonSettings>();
                if(button.Delete == false) {
                    list.Add(button);
                }
            }
            PdfButtons = list.OrderBy(x => x.Position);
        }
    }
}