using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CheckoutExtensionContext {
        public IValueProvider ValueProvider { get; set; }
        public ModelStateDictionary ModelState { get; set; }
        public FormCollection FormCollection { get; set; }
    }
}