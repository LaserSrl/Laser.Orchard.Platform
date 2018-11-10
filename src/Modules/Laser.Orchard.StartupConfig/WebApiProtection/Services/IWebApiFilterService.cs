using Laser.Orchard.StartupConfig.Services;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.WebApiProtection.Services {
    public interface IWebApiFilterService : IDependency {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAction">Delegate method executing the default action for Authorized Api</param>
        /// <param name="unauthorizedResponseAction">Delegate method executing the action for Unauthorized Api: input string parameters contains the unauthorized message</param>
        /// <param name="workContext">Delegate function returning an Orchard WorkContext object</param>
        /// <param name="protectAlways">A boolean value telling if protection should be always applied or applied according to WebApiProtecion Site settings</param>
        void ApplyFilter(Action baseAction, Action<string> unauthorizedResponseAction, Func<WorkContext> workContext, bool protectAlways);
    }

    public class DefaultWebApiFilter : IWebApiFilterService {
        private string _additionalCacheKey;

        public DefaultWebApiFilter() {
            _additionalCacheKey = null;
        }

        public void ApplyFilter(Action baseAction, Action<string> unauthorizedResponseAction, Func<WorkContext> workContext, bool protectAlways) {
            IApiKeyService apiKeyService = null;
            if (workContext().TryResolve<IApiKeyService>(out apiKeyService)) {
                _additionalCacheKey = apiKeyService.ValidateRequestByApiKey(_additionalCacheKey, protectAlways);
                if ((_additionalCacheKey != null) && (_additionalCacheKey != "AuthorizedApi")) {
                    var result = "UnauthorizedApi";
                    unauthorizedResponseAction(result);
                } else {
                    baseAction();
                }
            } else {
                baseAction();
            }
        }
    }
}