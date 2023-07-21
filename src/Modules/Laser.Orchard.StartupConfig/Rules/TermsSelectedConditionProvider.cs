using Laser.Orchard.StartupConfig.Services;
using Orchard.Conditions.Services;
using Orchard.Taxonomies.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Rules {
    /// <summary>
    /// Rules in Widget Layer Definition: 
    /// TaxonomyFieldHasAnyTerms('PartName', 'FieldName', 'csv of ids')
    /// TaxonomyFieldHasAllTerms('PartName', 'FieldName', 'csv of ids')
    /// 
    /// These rules will test whether specific terms, from a comma-separated
    /// list of their ids, are selected in TaxonomyField of the current
    /// ContentItem, identified by its Technical Name and the name of the
    /// ContentPart that contains it. That means for example that if a field
    /// is added directly to the definition of ContentType CT1, the first 
    /// argument to this rules should be CT1.
    /// 
    /// If the current ContentItem doesn't have that field, these rules
    /// will return false.
    /// 
    /// For the "Any" rule, the list of ids is optional: if it's missing of empty, 
    /// any selected value will satisfy the condition.
    /// 
    /// </summary>
    public class TermsSelectedConditionProvider : IConditionProvider {
        private readonly ICurrentContentAccessor _currentContentAccessor;

        public TermsSelectedConditionProvider(
            ICurrentContentAccessor currentContentAccessor) {

            _currentContentAccessor = currentContentAccessor;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            switch (evaluationContext.FunctionName.ToLowerInvariant()) {
                case "taxonomyfieldhasanyterms":
                    if (evaluationContext.Arguments.Length == 2 || evaluationContext.Arguments.Length == 3) {
                        ProcessAnyCondition(evaluationContext);
                    }
                    break;
                case "taxonomyfieldhasallterms":
                    if (evaluationContext.Arguments.Length == 3) {
                        ProcessAllCondition(evaluationContext);
                    }
                    break;
                default:
                    break;
            }
        }

        private void ProcessAnyCondition(ConditionEvaluationContext evaluationContext) {
            // parse arguments
            var partName = evaluationContext.Arguments[0].ToString();
            var fieldName = evaluationContext.Arguments[1].ToString();
            // the third argument to the rule is the list of Ids
            var termIds = evaluationContext.Arguments.Length == 2
                ? ""
                : evaluationContext.Arguments[2].ToString();
            var ids = termIds
                .Split(new[] { ',' })
                .Select(s => {
                    int a;
                    if (int.TryParse(s, out a)) {
                        return a;
                    }
                    return 0;
                })
                .Where(i => i > 0);
            // validate
            if (string.IsNullOrWhiteSpace(partName) || string.IsNullOrWhiteSpace(fieldName)) {
                // sanity check
                evaluationContext.Result = false;
                return;
            }
            var field = GetTaxonomyField(partName, fieldName);
            if (field == null) {
                // current content doesn't have a field like this
                evaluationContext.Result = false;
                return;
            }
            // evaluate
            if (ids.Any()) {
                // if there were Ids, at least one has to be selected
                evaluationContext.Result = field.Terms.Any(
                    t => ids.Contains(t.Id));
            } else {
                // If no Id was provided, any selected Id is fine
                evaluationContext.Result = field.Terms.Any();
            }
        }

        private void ProcessAllCondition(ConditionEvaluationContext evaluationContext) {
            // parse arguments
            var partName = evaluationContext.Arguments[0].ToString();
            var fieldName = evaluationContext.Arguments[1].ToString();
            // the third argument to the rule is the list of Ids and is mandatory
            var termIds = evaluationContext.Arguments[2].ToString();
            var ids = termIds
                .Split(new[] { ',' })
                .Select(s => {
                    int a;
                    if (int.TryParse(s, out a)) {
                        return a;
                    }
                    return 0;
                })
                .Where(i => i > 0);
            // validate
            if (string.IsNullOrWhiteSpace(partName) || string.IsNullOrWhiteSpace(fieldName)) {
                // sanity check
                evaluationContext.Result = false;
                return;
            }
            var field = GetTaxonomyField(partName, fieldName);
            if (field == null) {
                // current content doesn't have a field like this
                evaluationContext.Result = false;
                return;
            }
            if (!ids.Any()) {
                // some valid ids is required
                evaluationContext.Result = false;
                return;
            }
            // evaluate
            var selectedIds = field.Terms.Select(t => t.Id);
            evaluationContext.Result = ids.All(i => selectedIds.Contains(i));
        }

        private TaxonomyField GetTaxonomyField(string partName, string fieldName) {
            var currentContent = _currentContentAccessor.CurrentContentItem;
            if (currentContent == null) {
                return null;
            }
            // get the part
            var part = currentContent.Parts
                .FirstOrDefault(pa => pa.PartDefinition.Name == partName);
            if (part == null) {
                return null;
            }
            // get the field
            var field = part.Fields
                .FirstOrDefault(fi => fi is TaxonomyField
                    && fieldName.Equals(fi.Name, StringComparison.OrdinalIgnoreCase))
                as TaxonomyField;
            return field;
        }
    }
}