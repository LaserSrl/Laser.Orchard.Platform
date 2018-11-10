using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    /// <summary>
    /// Implement this abstract class in controllers designed to manage settings for multi-step
    /// authentication steps.
    /// </summary>
    [OrchardFeature("Laser.Orchard.MultiStepAuthentication")]
    public abstract class BaseMultiStepAdminController : Controller, IUpdateModel, IMultiStepAdminController {
        public virtual string ActionName {
            get { return "Index"; }
        }

        public virtual string ControllerName {
            get {
                var name = this.GetType().Name;
                if (name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase)) {
                    name = name.Substring(0, name.Length - "controller".Length);
                }
                return name;
            }
        }

        public virtual string AreaName {
            get {
                return this.GetType().Assembly.GetName().Name;
            }
        }

        public abstract LocalizedString Caption { get; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}