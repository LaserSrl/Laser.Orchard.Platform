using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Services {
    public class DefaultContentGDPRManager : IContentGDPRManager {
        
        private readonly Lazy<IEnumerable<IContentGDPRHandler>> _GDPRhandlers;
        private readonly Lazy<IEnumerable<IContentHandler>> _contentHandlers;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IGDPRProcessAllowedProvider> _GDPRProcessAllowedProviders;


        public DefaultContentGDPRManager(
            Lazy<IEnumerable<IContentGDPRHandler>> gdprHandlers,
            Lazy<IEnumerable<IContentHandler>> contentHandlers,
            IContentManager contentManager,
            IEnumerable<IGDPRProcessAllowedProvider> GDPRProcessAllowedProviders) {

            _GDPRhandlers = gdprHandlers;
            _contentHandlers = contentHandlers;
            _contentManager = contentManager;
            _GDPRProcessAllowedProviders = GDPRProcessAllowedProviders;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<IContentGDPRHandler> GDPRHandlers {
            get { return _GDPRhandlers.Value; }
        }
        public IEnumerable<IContentHandler> ContentHandlers {
            get { return _contentHandlers.Value; }
        }

        /*
         * The sequence of events for GDPR processing is designed as follows:
         *  - Updating
         *  - Anonymizing/Erasing
         *  - Updated
         *  - Publish (conditionally)
         *  - Anonymized/Erased
         * This allows all normal handlers to be notified of the fact that the content is being affected,
         * as well as the workflows happening in the Anonymized/Erased phase to be able to do their stuff.
         * A typical example as to why the Anonymized/Erased events should be last has to do with having to
         * delete the ContentItem being processed: if this was done earlier, the other handlers would be 
         * unable to take care of their stuff, and, for example, the records for fields would not be updated
         * correctly, thus making the process non compliant with the GDPR.
         * Nowhere there are specific guarantees regarding the order of the handlers for a specific call:
         * Orchard attempts to enforce an order given by feature dependencies, but in case there are no
         * specific dependencies in the module.txt files, the order of handlers cannot be assumed to be
         * consistent.
         * */

        /*
         * Any intelligence related to the processes over items should go here in the manager. The handlers
         * will take care of the implementation details for every little thing. Here in the manager we are
         * responsible for making the call as to whether the ContentItems over which the processes were
         * invoked should actually be anonymized/erased.
         * For example, this is the place to check whether a specific time has passed since the creation/update
         * of the content item.
         * Luckily, we have the Process method, that can take care of testing all these conditions. The most
         * flexible and expandable way is then to put all those checks in a bunch of providers, each giving a
         * boolean result on the verification of a specific condition to processing, and all of them combined
         * with a logic AND operator. What this means is that it is enough for a single operator to "fail" and
         * return false for the process to stop entirely.
         * */

        public void Anonymize(ContentItem contentItem) {
            Anonymize(new AnonymizeContentContext(contentItem));
        }

        public void Anonymize(ContentItem contentItem, GDPRContentContext previousContext) {
            Anonymize(new AnonymizeContentContext(contentItem, previousContext));
        }

        private void Anonymize(AnonymizeContentContext context) {
            Process(context, (ctx, action) => {
                // Invoke Anonymizing handlers before anonymization
                GDPRHandlers.Invoke(handler => handler.Anonymizing(ctx), Logger);

                action(ctx);
                // Do any anonymization operation on the ContentItem object itself

                // Invoke Anonymized handlers after we are done.
                GDPRHandlers.Invoke(handler => handler.Anonymized(ctx), Logger);
            });
        }
        
        public void Erase(ContentItem contentItem) {
            Erase(new EraseContentContext(contentItem));
        }

        public void Erase(ContentItem contentItem, GDPRContentContext previousContext) {
            Erase(new EraseContentContext(contentItem, previousContext));
        }

        private void Erase(EraseContentContext context) {

            Process(context, (ctx, action) => {
                // Invoke Erasing handlers before erasure
                GDPRHandlers.Invoke(handler => handler.Erasing(ctx), Logger);

                action(ctx);
                // Do any erasure operation on the ContentItem object itself

                // Invoke Erased handlers after we are done.
                GDPRHandlers.Invoke(handler => handler.Erased(ctx), Logger);

                // we will handle the DeleteItemsAfterErasure setting here rather than in a handler
                // because we want to make sure it is the last thing. If we put that in a handler, 
                // it may execute "out of order" for something else.
                if (context.GDPRPart // if this were null, we wouldn't even be here
                    ?.TypePartDefinition.Settings.GetModel<GDPRPartTypeSettings>()
                    ?.DeleteItemsAfterErasure == true) {
                    // Delete the item
                    _contentManager.Remove(context.ContentItem);
                }
            });
        }

        /// <summary>
        /// This method is a wrapper for the actual processing, that is passed as the Action parameter
        /// to this method. This method takes care of any step or check that may be shared among the different
        /// processing methods. Those include invoking Updating and Updated handlers on the ContentItem
        /// respectively before and after running the process for GDPR.
        /// </summary>
        /// <typeparam name="TCtx">The type of the context being used. This may be any implementation of
        /// GDPRContentContext.</typeparam>
        /// <param name="context">The context object used for the processing.</param>
        /// <param name="GDPRProcess">The action that will do the actual processing based on the context.</param>
        private void Process<TCtx>(
            TCtx context, 
            Action<TCtx, Action<TCtx>> GDPRProcess)
            where TCtx : GDPRContentContext {

            // Here test for all providers that may prevent us from processing the current item.
            // It's enough for one of them to report false, for this method to set the process outcome
            // to Protected and return without invoking any of the handlers.
            if (_GDPRProcessAllowedProviders.Any(gpa => !gpa.ProcessIsAllowed(context))) {
                context.Outcome = GDPRProcessOutcome.Protected;
                return;
            }
            
            var updateContext = new UpdateContentContext(context.ContentItem);
            ContentHandlers.Invoke(handler => handler.Updating(updateContext), Logger);

            GDPRProcess(context, PostProcessing);
        }

        /// <summary>
        /// These operations should all happen before the Anonymized/Erased event is raised. Otherwise
        /// we may end up deleting the item in a Workflow there, and preventing all other handlers
        /// to run as they should.
        /// </summary>
        /// <param name="context"></param>
        private void PostProcessing(GDPRContentContext context) {
            var updateCtx = new UpdateContentContext(context.ContentItem);
            ContentHandlers.Invoke(handler => handler.Updated(updateCtx), Logger);

            // Re-publish the published version of the ContentItem, if it exists.
            // This way, all its handlers are launched again, synchronizing information as needed.
            // For example, this is sufficient to propagate anonymization onto the Alias, when 
            // AutoroutePart is subject to anonymization/erasure, and onto the FieldIndexRecords 
            // for fields that we processed above.
            var publishedVersion = context.AllVersions
                .FirstOrDefault(v => v.VersionRecord.Published);
            if (publishedVersion != null) {
                var civr = publishedVersion.Record.Versions.FirstOrDefault(v => v.Published);
                civr.Published = false;
                _contentManager.Publish(publishedVersion);
            }
        }

    }
}