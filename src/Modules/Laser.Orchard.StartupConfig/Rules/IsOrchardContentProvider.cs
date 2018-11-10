using System;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Widgets.Services;
using Orchard.Localization.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Conditions.Services;

namespace Laser.Orchard.StartupConfig.Rules {
    public class IsOrchardContentProvider : IConditionProvider {
        private readonly ICurrentContentAccessor _currentContentAccessor;


        public IsOrchardContentProvider(ICurrentContentAccessor currentContentAccessor) {
            _currentContentAccessor = currentContentAccessor;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (!String.Equals(evaluationContext.FunctionName, "IsOrchardContent", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var matches = _currentContentAccessor.CurrentContentItemId.HasValue;
            if (matches) {
                evaluationContext.Result = true;
                return;
            }

            evaluationContext.Result = false;
            return;

        }
    }
}