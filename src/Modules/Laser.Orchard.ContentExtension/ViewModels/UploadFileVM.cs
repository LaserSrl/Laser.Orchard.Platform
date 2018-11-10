using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ContentExtension.ViewModels {
    public class UploadFileVM {
        public UploadFileVM() {
            ElencoUrl = new List<string>();
            ElencoId = new List<int>();
            IdField = "";
            FileNumber = -1;
            FolderField = "";
            SubFolder = "";
        }
        public List<string> ElencoUrl { get; set; }
        public List<int> ElencoId { get; set; }
        public string IdField { get; set; }
        public string FolderField { get; set; }
        public string SubFolder { get; set; }
        public int FileNumber { get; set; }
    }
}