using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Services {
    public class ControllerContextAccessor:IControllerContextAccessor {
        private ControllerContext _context;

        public System.Web.Mvc.ControllerContext Context {
            get {
                return _context;
            }
            set {
                _context=value;
            }
        }
    }
}