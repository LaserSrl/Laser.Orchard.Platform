using Laser.Orchard.Vimeo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.ViewModels {
    public class VimeoAccessTokenViewModel {
        public int Id { get; set; }
        public string AccessToken { get; set; }

        public bool Delete { get; set; }
    }
}