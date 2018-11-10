using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.ShareLink.ViewModels {
    public class OpenGraphVM {
        public OpenGraphVM() {
            TwitterCard = "summary_large_image";
            Type = "website";


        }


        public string Title { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public string Site_name { get; set; }
        public string Description { get; set; }
        public string Fbapp_id { get; set; }
        
        #region Twitter Card

        public string TwitterCard { get; set; }

        /// <summary>
        /// Valorizzato con l'utente twitter inserito nell'open authentication
        /// </summary>
        public string TwitterSite { get; set; }

        /// <summary>
        /// attualmente non viene utilizzato
        /// </summary>
        public string TwitterCreator { get; set; }


        public string TwitterTitle { get; set; }


        public string TwitterDescription { get; set; }

        public string TwitterImage { get; set; }
        #endregion
    }
}

