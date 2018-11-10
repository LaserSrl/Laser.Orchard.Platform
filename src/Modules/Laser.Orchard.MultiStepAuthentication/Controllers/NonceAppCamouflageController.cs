using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.MultiStepAuthentication.Services;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceAppCamouflageController:Controller {
        private readonly INonceLinkProvider _noncelinkProvider;
            public NonceAppCamouflageController(INonceLinkProvider noncelinkProvider) {
            _noncelinkProvider = noncelinkProvider;
        }
        public RedirectResult GetByURL(string n) {
            return Redirect(_noncelinkProvider.FormatURI(n));
        }
    }
}