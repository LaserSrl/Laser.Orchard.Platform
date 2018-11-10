using Laser.Orchard.GDPR.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Activities {
    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public abstract class GDPRProcessTask : Task {

        protected readonly IContentGDPRManager _contentGDPRManager;
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager;

        public GDPRProcessTask(
            IContentGDPRManager contentGDPRManager,
            ITokenizer tokenizer,
            IContentManager contentManager) {

            _contentGDPRManager = contentGDPRManager;
            _tokenizer = tokenizer;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public override LocalizedString Category {
            get { return T("Content Items"); }
        }

        public override string Form {
            get { return "GDPRSelectContentItem"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("OK"), // everything was fine with the process
                T("Error"), // there was an error with the process
                T("NoItem"), // There was no item to process
            };
        }

        public virtual Action<ContentItem> GDPRProcess { get; protected set; }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override IEnumerable<LocalizedString> Execute(
            WorkflowContext workflowContext, ActivityContext activityContext) {

            // Get the ContentItems form the context
            var contentItems = GetItems(workflowContext, activityContext);
            if (!contentItems.Any()) {
                yield return T("NoItem");
            }

            var errorMessages = workflowContext.GetState<string>("ErrorMessage");
            errorMessages = string.IsNullOrWhiteSpace(errorMessages) ? "" : errorMessages + Environment.NewLine;

            var error = false;
            foreach (var item in contentItems) {
                try {
                    // for each ContentItem, invoke the process
                    GDPRProcess(item);
                } catch (Exception ex) {
                    error = true;
                    var msg = T("Error during the execution of {0} for ContentItem with Id {1}",
                            Name, item.Id).Text;
                    // log the exception
                    Logger.Error(ex, msg);
                    // add information on the error to the WorkflowContext
                    errorMessages += msg + Environment.NewLine;
                }
            }

            if (error) {
                workflowContext.SetState("ErrorMessage", errorMessages);
                yield return T("Error");
            }

            yield return T("OK");
        }

        private IEnumerable<ContentItem> GetItems(
            WorkflowContext workflowContext, ActivityContext activityContext) {
            // Get the ContentItem Ids from the form
            var idString = activityContext.GetState<string>("ContentItemId") ?? "";
            idString = _tokenizer.Replace(idString, workflowContext.Tokens);
            if (string.IsNullOrWhiteSpace(idString)) {
                // in this case we try to get the ContentItem from the WorkflowContext
                if (workflowContext.Content?.ContentItem != null) {
                    return new List<ContentItem>() { workflowContext.Content.ContentItem };
                }
                // if we are here there is nothing to do but return an empty Enumerable.
                return Enumerable.Empty<ContentItem>();
            }
            var ids = idString.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    int i = 0;
                    if (int.TryParse(s, out i)) {
                        return i;
                    }
                    return 0;
                })
                .Where(i => i > 0)
                .Distinct();
            // Using the Ids, get the ContentItems
            if (ids == null || !ids.Any()) {
                return Enumerable.Empty<ContentItem>();
            }
            return _contentManager.GetMany<ContentItem>(ids, VersionOptions.Latest, QueryHints.Empty);
        }
    }

    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class AnonymizationProcessTask : GDPRProcessTask {

        public AnonymizationProcessTask(
            IContentGDPRManager contentGDPRManager,
            ITokenizer tokenizer,
            IContentManager contentManager) 
            : base(contentGDPRManager,
                  tokenizer,
                  contentManager) {

            GDPRProcess = _contentGDPRManager.Anonymize;
        }

        public override LocalizedString Description {
            get {
                return T("Anonymize the selected ContentItems");
            }
        }

        public override string Name {
            get {
                return "AnonymizeContentItems";
            }
        }
        
    }

    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class ErasureProcessTask : GDPRProcessTask {

        public ErasureProcessTask(
            IContentGDPRManager contentGDPRManager,
            ITokenizer tokenizer,
            IContentManager contentManager)
            : base(contentGDPRManager,
                  tokenizer,
                  contentManager) {

            GDPRProcess = _contentGDPRManager.Erase;
        }

        public override LocalizedString Description {
            get {
                return T("Erase the selected ContentItems");
            }
        }

        public override string Name {
            get {
                return "EraseContentItems";
            }
        }

    }
}